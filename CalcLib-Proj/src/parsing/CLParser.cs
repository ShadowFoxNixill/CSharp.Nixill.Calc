using System;
using System.Collections.Generic;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;

namespace Nixill.CalcLib.Parsing {
  /// <summary>
  /// A class that turns a <c>List</c> of pieces into a full tree of
  ///   <c>CalcObject</c>s.
  /// </summary>
  public class CLParser {
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
    public static CalcObject ParseChain(List<CLObjectPiece> pieces) {
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
            string check = piece.Contents.Substring(1, piece.Contents.Length - 1);
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
            exp.RightObj = obj;
          }

          continue;
        }

        // Otherwise, this piece must be an operator.
        CLExpressionBuilder expNew = new CLExpressionBuilder();
        CLOperator op = null;
        valueLast = false;

        if (piece.Type == CLObjectPieceType.BinaryOperator) op = CLBinaryOperator.Get(piece.Contents);
        else if (piece.Type == CLObjectPieceType.PrefixOperator) op = CLPrefixOperator.Get(piece.Contents);
        else if (piece.Type == CLObjectPieceType.PostfixOperator) op = CLPostfixOperator.Get(piece.Contents);

        expNew.Operator = op;

        // If it's the first operator...
        if (exps.Count == 0) {
          // ... use the held value if one exists
          if (hold != null) expNew.LeftObj = hold;
          exps.AddLast(expNew);
        }
        // Otherwise...
        else {
          // For prefix operators we don't need to check priorities to the
          // left. They can just stack on in.
          if (op is CLPrefixOperator) {
            exps.Last.Value.RightExp = expNew;
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
                  !CLOperator.IsFromRight(op.Priority))
              ) // see line 281 https://github.com/ShadowFoxNixill/DiceCalculator/blob/master/src/main/java/net/nixill/dice/parsing/ExpressionParser.java#L281
            }
          }
        }
      }
    }
  }

  /// <summary>
  /// A mutable opbject with which to build a <c>CalcObject</c>.
  /// </summary>
  internal class CLExpressionBuilder {
    public CalcObject LeftObj;
    public CalcObject RightObj;
    public CLOperator Operator;
    public CLExpressionBuilder LeftExp;
    public CLExpressionBuilder RightExp;

    /// <summary>
    /// Builds the <c>CalcOperation</c>.
    /// </summary>
    public CalcOperation Build() {
      if (Operator == null) throw new InvalidOperationException("CalcOperations cannot be built without an operator.");

      string operType = Operator.GetType().Name;

      // Postfix and Binary operators require a left operand.
      if (!(Operator is CLPrefixOperator)) {
        if (LeftExp != null) LeftObj = LeftExp.Build();
        if (LeftObj == null) throw new InvalidOperationException(operType + " requires a left-side operand.");
      }
      else LeftObj = null;

      // Prefix and Binary operators require a left operand.
      if (!(Operator is CLPostfixOperator)) {
        if (RightExp != null) RightObj = RightExp.Build();
        if (RightObj == null) throw new InvalidOperationException(operType + " requires a right-side operand.");
      }
      else RightObj = null;

      return new CalcOperation(LeftObj, Operator, RightObj);
    }
  }
}