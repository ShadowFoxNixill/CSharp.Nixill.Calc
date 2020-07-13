using System.Text.RegularExpressions;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Operators;
using Nixill.Utils;

namespace Nixill.CalcLib.Parsing {
  /// <summary>
  /// Provides a convenience method to lex and parse simultaneously.
  /// </summary>
  public class CLInterpreter {
    /// <summary>
    /// Lexes and parses the input (simply by running
    ///   <c>CLParser.Parse</c> on the output of <c>CLLexer.Lex</C>).
    /// </summary>
    /// <param name="input">The express ion to lex and parse.</param>
    public static CalcObject Interpret(string input) => CLParser.Parse(CLLexer.Lex(input));
  }

  /// <summary>
  /// Lexes the input (splitting it into separate pieces)
  /// </summary>
  public class CLLexer {
    private static Regex rgxWhitespace = new Regex(@"^[ `\t\n]");
    private static Regex rgxNumber = new Regex(@"^(0|[1-9]\d*)(\.\d+)?");
    private static Regex rgxSeparator = new Regex(@"^[\(\)\[\]\,\}]");
    private static Regex rgxComment = new Regex(@"^\{\%[^\}]*}");
    private static Regex rgxName = new Regex(@"^\{([\$\_\^\!]?[a-zA-Z]([a-zA-Z\_\-0-9]*[a-zA-Z0-9])?|\d+)");
    private static Regex rgxString = new Regex(@"^\""([^\\\""]|\\[^A-Za-z0-9])*\""");
    private static Regex rgxOperator = new Regex(@"^[a-z\<\>\/\?\|\~\!\#\$\%\^\&\*\-\=\+ `\t\n]+");

    private static Regex rgxWhitespaceReplace = new Regex(@"[ `\t\n]");

    public static List<CLObjectPiece> Lex(string input) {
      List<CLObjectPiece> ret = new List<CLObjectPiece>();
      string last = "";
      int pos = 0;

      while (input.Length > 0) {
        int move = 0;
        Match match = null;

        // See if it's whitespace before any tokens.
        if (CLUtils.RegexMatches(rgxWhitespace, input, out match)) {
          move = match.Length;
        }

        // Is it a number?
        else if (CLUtils.RegexMatches(rgxNumber, input, out match)) {
          last = match.Value;
          ret.Add(new CLObjectPiece(last, CLObjectPieceType.Number, pos));
          move = match.Length;
        }

        // Is it a separator?
        else if (CLUtils.RegexMatches(rgxSeparator, input, out match)) {
          last = match.Value;
          ret.Add(new CLObjectPiece(last, CLObjectPieceType.Number, pos));
          move = match.Length;
        }

        // Is it a comment?
        else if (CLUtils.RegexMatches(rgxComment, input, out match)) {
          move = match.Length;
          // comments aren't included in the syntax tree
        }

        // Is it a name?
        else if (CLUtils.RegexMatches(rgxName, input, out match)) {
          last = match.Value;
          ret.Add(new CLObjectPiece(last, CLObjectPieceType.FunctionName, pos));
          move = match.Length;
        }

        // ... Is it an operator, or group thereof?
        else if (CLUtils.RegexMatches(rgxOperator, input, out match)) {
          string opers = rgxWhitespaceReplace.Replace(match.Value, "");
          move = match.Length;

          string next = "";
          if (input.Length > move) next = input.Substring(move, move + 1);

          // If the operator immediately follows a separator,
          //   it must be a prefix operator.
          bool prefix = (last == "" || last == "[" || last == "," || last == "(");

          // If the operator immediately precedes a separator,
          //   it must be a postfix operator.
          bool postfix = (next == "" || next == "]" || next == "," || next == ")" || next == "}");

          // It can't be both (as in the only thing between two separators).
          if (prefix && postfix) throw new CLSyntaxException("A value was expected between brackets.", pos);

          // There might be multiple operators in a row, so let's be sure to get them all.
          List<CLObjectPiece> pcs = CLOperators.GetOpers(opers, prefix, postfix, pos);
          ret.AddRange(pcs);

          last = match.Value;
        }

        // No match? That's a paddlin.
        else throw new CLSyntaxException("I don't know what this means.", pos);
      }

      return ret;
    }
  }
}