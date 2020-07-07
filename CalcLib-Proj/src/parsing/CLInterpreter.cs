using Nixill.CalcLib.Objects;

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
  public class CLLexer {

  }
}