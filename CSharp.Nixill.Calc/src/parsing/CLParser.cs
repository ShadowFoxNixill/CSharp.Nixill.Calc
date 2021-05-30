using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;

namespace Nixill.CalcLib.Parsing {
  /// <summary>
  /// A class that turns a <c>List</c> of pieces into a full tree of
  ///   <c>CalcObject</c>s.
  /// </summary>
  public class CLParser {
    private static Regex rgxName = new Regex(@"^\{(?:[ `\t\n])*([\$_\^\!]?[a-zA-Z]([a-zA-Z_\-0-9]*[a-zA-Z0-9])?|\d+)");

    /// <summary>
    /// The method that turns a <c>List</c> of pieces into a full tree of
    ///   <c>CalcObject</c>s.
    /// </summary>
    /// <param name="pieces">The list of pieces.</param>
    public static CalcObject Parse(List<CLObjectPiece> pieces) {
      CalcObject obj = ParseChain(pieces);

      if (pieces.Count > 0) {
        CLObjectPiece piece = pieces[0];
        throw new CLSyntaxException("Unmatched " + piece.Contents, piece.Position);
      }

      return obj;
    }

    // Parses a single "operation chain"
    private static CalcObject ParseChain(List<CLObjectPiece> pieces) {
      // First, we can't parse an empty list.
      if (pieces.Count == 0)
        throw new CLSyntaxException("Empty list received.", 0);

      LinkedList<CLExpressionBuilder> exps = new LinkedList<CLExpressionBuilder>();
      CalcObject hold = null;
      bool valueLast = false;

      // Loop through all the pieces now.
      while (pieces.Count != 0) {
        CLObjectPiece piece = pieces[0];

        // Get the next value, if there is one.
        CalcObject obj = null;
        bool err = false;

        // If it's a ()[]},
        if (piece.Type == CLObjectPieceType.Spacer) {
          if (piece.Contents == "(") {
            if (!valueLast) obj = ParseParentheses(pieces);
            else err = true;
          }
          else if (piece.Contents == "[") {
            if (!valueLast) obj = ParseList(pieces);
            else err = true;
          }
          else /* ], ), }, , */ {
            // Send control back up a parser level
            break;
          }
        }
        else if (piece.Type == CLObjectPieceType.FunctionName) {
          if (!valueLast) obj = ParseFunction(pieces);
          else err = true;
        }
        else if (piece.Type == CLObjectPieceType.Number) {
          if (!valueLast) {
            obj = new CalcNumber(Decimal.Parse(piece.Contents));
            pieces.RemoveAt(0);
          }
          else err = true;
        }
        else if (piece.Type == CLObjectPieceType.String) {
          if (!valueLast) {
            // Strip the quotes
            string check = piece.Contents.Substring(1, piece.Contents.Length - 2);
            // Parse special characters
            check = check.Replace(@"\\", "\uF000")
                .Replace(@"\n", "\n")
                .Replace(@"\t", "\t")
                .Replace(@"\", "")
                .Replace("\uF000", @"\");
            obj = new CalcString(check);
            pieces.RemoveAt(0);
          }
          else err = true;
        }

        if (err) throw new CLSyntaxException("Two consecutive values", piece.Position);

        // If there's a value, put it in the most recent expression
        if (obj != null) {
          valueLast = true;

          // If there's no expression, just hold the value.
          if (exps.Count == 0) hold = obj;
          else {
            // Put it on the most recent expression
            CLExpressionBuilder exp = exps.Last.Value;
            exp.Right = obj;
          }

          continue;
        }

        // Otherwise, this piece must be an operator.
        CLExpressionBuilder expNew = new CLExpressionBuilder();
        CLOperator op = null;
        valueLast = false;

        if (piece.Type == CLObjectPieceType.BinaryOperator) op = CLOperators.BinaryOperators[piece.Contents];
        else if (piece.Type == CLObjectPieceType.PrefixOperator) op = CLOperators.PrefixOperators[piece.Contents];
        else if (piece.Type == CLObjectPieceType.PostfixOperator) op = CLOperators.PostfixOperators[piece.Contents];

        expNew.Operator = op;

        // If it's the first operator...
        if (exps.Count == 0) {
          // ... use the held value if one exists
          if (hold != null) expNew.Left = hold;
          exps.AddLast(expNew);
        }
        // Otherwise...
        else {
          // For prefix operators we don't need to check priorities to the
          // left. They can just stack on in.
          if (op is CLPrefixOperator) {
            exps.Last.Value.Right = expNew;
            exps.AddLast(expNew);
          }
          else {
            CLExpressionBuilder expOld = null;
            CLExpressionBuilder expNext = null;
            // This code removes expressions from the stack that are a
            // higher priority than the one being removed, or that are
            // postfix.
            while (exps.Count != 0) {
              expNext = exps.Last.Value;
              if (
                // The next expression is a postfix expression.
                expNext.Operator is CLPostfixOperator ||
                // The next expression on the stack is still higher
                // priority
                expNext.Operator.Priority > op.Priority ||
                // The next expression on the stack is equal priority,
                // but evaluated left-to-right
                (expNext.Operator.Priority == op.Priority &&
                  !CLOperators.IsFromRight(op.Priority))
              ) {
                expOld = exps.Last.Value;
                exps.RemoveLast();
                expNext = null;
              }
              else {
                break;
              }
            }

            // The last removed expression becomes the left of this one.
            // (If there's no such expression, then the right of the next
            // expression on the stack becomes our left.)
            if (expOld != null) {
              expNew.Left = expOld;
            }
            else {
              expNew.Left = expNext.Right;
            }

            // Then, this expression becomes the right of the next one.
            if (expNext != null) {
              expNext.Right = expNew;
            }

            exps.AddLast(expNew);
          }
        }

        pieces.RemoveAt(0);
      }

      if (exps.Count == 0) return hold;
      else return exps.First.Value.Build();
    }

    // Parses a parenthesized expression.
    private static CalcObject ParseParentheses(List<CLObjectPiece> pieces) {
      // First, get the "(" that spawned this method.
      CLObjectPiece lpar = pieces[0];
      pieces.RemoveAt(0);

      CalcObject ret = ParseChain(pieces);

      // If we now have an empty expression, we never closed the "(".
      if (pieces.Count == 0) throw new CLSyntaxException("Unmatched (", lpar.Position);
      else {
        // Otherwise, we should have a ")" next.
        CLObjectPiece rpar = pieces[0];
        pieces.RemoveAt(0);
        if (rpar.Contents != ")")
          // Maybe it's a "]", "}", or "," though.
          throw new CLSyntaxException("Unmatched ( and " + rpar.Contents, lpar.Position);
      }

      // But if we've made it here, we're fine.
      return ret;
    }

    // Parses a list.
    private static CalcListExpression ParseList(List<CLObjectPiece> pieces) {
      // First, get the "[" that spawned this method.
      CLObjectPiece lbracket = pieces[0];
      pieces.RemoveAt(0);

      List<CalcObject> ret = new List<CalcObject>();

      // Also, we'll allow empty lists.
      CLObjectPiece next = pieces[0];
      if (next.Contents == "]") {
        pieces.RemoveAt(0);
        return new CalcListExpression(ret.ToArray());
      }

      // But anyway, let's start populating non-empty lists.
      while (pieces.Count != 0) {
        // Get the value
        CalcObject obj = ParseChain(pieces);

        // But no empty values are allowed
        if (obj == null) throw new CLSyntaxException("Value expected in list", pieces[0].Position);

        // Add it to the list
        ret.Add(obj);

        // Now get the next bracket
        next = pieces[0];
        pieces.RemoveAt(0);

        // If it's a ")" or "}", error.
        if (next.Contents == ")" || next.Contents == "}")
          throw new CLSyntaxException("Unclosed [ and " + next.Contents, lbracket.Position);

        // If it's a "]", we're done.
        if (next.Contents == "]")
          return new CalcListExpression(ret.ToArray());

        // Otherwise, keep looping.
      }

      // Oh, if we're out of pieces...
      throw new CLSyntaxException("Unclosed [", lbracket.Position);
    }

    // And this method parses Functions, including Code Functions.
    private static CalcFunction ParseFunction(List<CLObjectPiece> pieces) {
      // As with the other two methods, we'll get and store the opener.
      CLObjectPiece lbrace = pieces[0];
      pieces.RemoveAt(0);

      string opener = lbrace.Contents;

      List<CalcObject> pars = new List<CalcObject>();
      Match mtcName = rgxName.Match(opener);
      string name = mtcName.Groups[1].Value;

      // And again, no-param functions are allowed.
      CLObjectPiece next = pieces[0];
      if (next.Contents == "}") {
        pieces.RemoveAt(0);
        return new CalcFunction(name, pars.ToArray());
      }

      // If the next piece is a comma, let's just get rid of it.
      if (next.Contents == ",") pieces.RemoveAt(0);

      // And parse through the params.
      while (pieces.Count != 0) {
        // Get the value
        CalcObject obj = ParseChain(pieces);

        // But no empty values are allowed
        if (obj == null) throw new CLSyntaxException("Value expected in function", pieces[0].Position);

        // Add it to the params
        pars.Add(obj);

        // Now get the next bracket
        next = pieces[0];
        pieces.RemoveAt(0);

        // If it's a ")" or "]", error.
        if (next.Contents == ")" || next.Contents == "]")
          throw new CLSyntaxException("Unclosed { and " + next.Contents, lbrace.Position);

        // If it's a "}", we're done.
        if (next.Contents == "}")
          return new CalcFunction(name, pars.ToArray());

        // Otherwise, keep looping.
      }

      // Oh, if we're out of pieces...
      throw new CLSyntaxException("Unclosed {", lbrace.Position);
    }
  }

  // A mutable object with which to build a CalcObject.
  internal class CLExpressionBuilder {
    public CLOperator Operator;
    public CalcObject LeftObj { get; private set; }
    public CalcObject RightObj { get; private set; }
    public CLExpressionBuilder LeftExp { get; private set; }
    public CLExpressionBuilder RightExp { get; private set; }

    public object Left {
      get {
        if (LeftObj != null) return LeftObj;
        else return LeftExp;
      }
      set {
        if (value == null) {
          LeftObj = null;
          LeftExp = null;
        }
        else if (value is CalcObject obj) {
          LeftObj = obj;
          LeftExp = null;
        }
        else if (value is CLExpressionBuilder exp) {
          LeftExp = exp;
          LeftObj = null;
        }
        else throw new InvalidCastException("CLExpressionBuilder.Left only takes a CalcObject or CLExpressionBuilder");
      }
    }

    public object Right {
      get {
        if (RightObj != null) return RightObj;
        else return RightExp;
      }
      set {
        if (value == null) {
          RightObj = null;
          RightExp = null;
        }
        else if (value is CalcObject obj) {
          RightObj = obj;
          RightExp = null;
        }
        else if (value is CLExpressionBuilder exp) {
          RightExp = exp;
          RightObj = null;
        }
        else throw new InvalidCastException("CLExpressionBuilder.Right only takes a CalcObject or CLExpressionBuilder");
      }
    }

    /// <summary>
    /// Builds the <c>CalcOperation</c>.
    /// </summary>
    public CalcOperation Build() {
      if (Operator == null) throw new InvalidOperationException("CalcOperations cannot be built without an operator.");

      string operType = Operator.GetType().Name;

      // Postfix and Binary operators require a left operand.
      if (!(Operator is CLPrefixOperator)) {
        if (LeftExp != null) Left = LeftExp.Build();
        if (LeftObj == null) throw new InvalidOperationException(operType + " requires a left-side operand.");
      }
      else LeftObj = null;

      // Prefix and Binary operators require a left operand.
      if (!(Operator is CLPostfixOperator)) {
        if (RightExp != null) Right = RightExp.Build();
        if (RightObj == null) throw new InvalidOperationException(operType + " requires a right-side operand.");
      }
      else RightObj = null;

      return new CalcOperation(LeftObj, Operator, RightObj);
    }
  }
}