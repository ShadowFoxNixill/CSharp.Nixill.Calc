using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Parsing;
using Nixill.CalcLib.Varaibles;
using Nixill.Utils;

namespace Nixill.CalcLib.Operatorsv2 {
  public static class CLOperators {
    private static HashSet<int> FromRightOperators = new HashSet<int>();

    /// <summary>
    /// Sets a given priority level to group right-to-left.
    /// </summary>
    /// <param name="level">The level to set.</param>
    public static void SetFromRight(int level) {
      FromRightOperators.Add(level);
    }

    /// <summary>
    /// Sets a given priority level to group left-to-right.
    /// </summary>
    /// <param name="level">The level to set.</param>
    public static void UnsetFromRight(int level) {
      FromRightOperators.Remove(level);
    }

    /// <summary>
    /// Checks whether a given priority level groups right-to-left.
    /// </summary>
    /// <param name="level">The level to check.</param>
    public static bool IsFromRight(int level) => FromRightOperators.Contains(level);

    // internal static Dictionary<string, CLOperator> PrefixOperators = new Dictionary<string, CLOperator>();
    // internal static Dictionary<string, CLOperator> BinaryOperators = new Dictionary<string, CLOperator>();
    // internal static Dictionary<string, CLOperator> PostfixOperators = new Dictionary<string, CLOperator>();

    public static readonly CLOperatorList PrefixOperators = new CLOperatorList();
    public static readonly CLOperatorList BinaryOperators = new CLOperatorList();
    public static readonly CLOperatorList PostfixOperators = new CLOperatorList();

    private static string ptnPrefix;
    private static string ptnPostfix;

    private static Regex rgxPrefix;
    private static Regex rgxPostfix;
    private static Regex rgxCombined;
    private static Regex rgxSymbol = new Regex(@"([^a-zA-Z0-9])");
    internal static bool rgxInitiated = false;

    public static List<CLObjectPiece> GetOpers(string input, bool prefix, bool postfix, int pos) {
      // First, let's see if the whole thing matches an operator.
      if (prefix) {
        if (PrefixOperators.ContainsKey(input))
          return CLUtils.ListOfOne(new CLObjectPiece(input, CLObjectPieceType.PrefixOperator, pos));
      }
      else if (postfix) {
        if (PostfixOperators.ContainsKey(input))
          return CLUtils.ListOfOne(new CLObjectPiece(input, CLObjectPieceType.PostfixOperator, pos));
      }
      else {
        if (BinaryOperators.ContainsKey(input))
          return CLUtils.ListOfOne(new CLObjectPiece(input, CLObjectPieceType.BinaryOperator, pos));
      }

      // Otherwise let's go through the group to see if we can split it into many
      if (!rgxInitiated) InitRegexes();

      if (prefix) {
        return GetMultiOpers(input, ptnPrefix, CLObjectPieceType.PrefixOperator, ref pos);
      }
      else if (postfix) {
        return GetMultiOpers(input, ptnPostfix, CLObjectPieceType.PostfixOperator, ref pos);
      }
      else {
        List<CLObjectPiece> ret = new List<CLObjectPiece>();
        Match match;

        // Try to match against the whole thing
        if (CLUtils.RegexMatches(rgxCombined, input, out match)) {
          string sPostfix = match.Groups[1].Value;
          string sBinary = match.Groups[3].Value;
          string sPrefix = match.Groups[4].Value;

          // what postfix operator(s) do we have?
          if (sPostfix != "") {
            if (PostfixOperators.ContainsKey(sPostfix)) {
              ret.Add(new CLObjectPiece(sPostfix, CLObjectPieceType.PostfixOperator, pos));
              pos += sPostfix.Length;
            }
            else {
              ret.AddRange(GetMultiOpers(sPostfix, ptnPostfix, CLObjectPieceType.PostfixOperator, ref pos));
            }
          }

          // what binary operator do we have?
          ret.Add(new CLObjectPiece(sBinary, CLObjectPieceType.BinaryOperator, pos));
          pos += sBinary.Length;

          // what prefix operator(s) do we have?
          if (sPrefix != "") {
            if (PrefixOperators.ContainsKey(sPrefix)) {
              ret.Add(new CLObjectPiece(sPrefix, CLObjectPieceType.PrefixOperator, pos));
              pos += sPrefix.Length;
            }
            else {
              ret.AddRange(GetMultiOpers(sPrefix, ptnPrefix, CLObjectPieceType.PrefixOperator, ref pos));
            }
          }

          return ret;
        }

        // We didn't match a valid operator group
        throw new CLSyntaxException("I don't know what the operator " + input + " is.", pos);
      }
    }

    // Returns a chain of multiple prefix or postfix operators 
    private static List<CLObjectPiece> GetMultiOpers(string text, string ptnRegex, CLObjectPieceType pieceType, ref int pos) {
      string multiPattern = ptnRegex;
      Match match;

      for (int i = 2; i <= text.Length; i++) {
        multiPattern += ptnRegex;
        Regex ptnTest = new Regex("^" + multiPattern + "$");

        if (CLUtils.RegexMatches(ptnTest, text, out match)) {
          List<CLObjectPiece> ret = new List<CLObjectPiece>();
          for (int j = 1; j <= i; j++) {
            ret.Add(new CLObjectPiece(match.Groups[j].Value, pieceType, pos));
            pos += match.Groups[j].Value.Length;
          }

          return ret;
        }
      }

      throw new CLSyntaxException("I don't know what the operator " + text + " is.", pos);
    }

    // Takes the keys from a dictionary and turns them into a regex that matches any one key
    private static string KeysToPattern(CLOperatorList dict) {
      string ret = "";
      foreach (string key in dict.Keys) {
        ret += "|" + rgxSymbol.Replace(key, @"\\$1");
      }
      return "(" + ret.Substring(1) + ")";
    }

    // Initializes regexes with the existing keys
    private static void InitRegexes() {
      string ptnPrefix = KeysToPattern(PrefixOperators);
      rgxPrefix = new Regex(ptnPrefix);

      string ptnPostfix = KeysToPattern(PostfixOperators);
      rgxPostfix = new Regex(ptnPostfix);

      string ptnBinary = KeysToPattern(BinaryOperators);
      rgxCombined = new Regex("(" + ptnPostfix + "*)" + ptnBinary + "(" + ptnPrefix + "*)");

      rgxInitiated = true;
    }
  }

  public class CLOperatorList {
    private Dictionary<string, CLOperator> OperList = new Dictionary<string, CLOperator>();

    public CLOperator this[string key] {
      get => OperList[key];
      internal set => OperList[key] = value;
    }

    public bool ContainsKey(string key) => OperList.ContainsKey(key);

    public Dictionary<string, CLOperator>.KeyCollection Keys => OperList.Keys;
  }

  public abstract class CLOperator {
    internal CLOperator() {
      if (this is CLBinaryOperator bin) CLOperators.BinaryOperators[bin.Symbol] = bin;
      if (this is CLPrefixOperator pre) CLOperators.PrefixOperators[pre.Symbol] = pre;
      if (this is CLPostfixOperator post) CLOperators.PostfixOperators[post.Symbol] = post;
      CLOperators.rgxInitiated = false;
    }

    /// <summary>The priority of a given <c>CLOperator</c>.</summary>
    public int Priority { get; protected set; }
    /// <summary>The symbol of a given <c>CLOperator</c>.</summary>
    public string Symbol { get; protected set; }
    /// <summary>
    /// Whether or not the <c>CLOperator</c> evaluates
    /// <c>CLExpression</c>s (<c>false</c>) or just their resultant values
    /// (<c>true</c>).
    /// </summary>
    public bool ValueBased { get; protected set; }

    /// <summary>
    /// Returns a string representation of this <c>CLOperator</c>.
    /// </summary>
    public abstract override string ToString();
  }

  /// <summary>
  /// Represents a binary (two-operand) <c>CLOperator</c>.
  /// </summary>
  public class CLBinaryOperator : CLOperator {
    private Dictionary<Type, Dictionary<Type, CLBinaryOperatorFunc>> Operators;

    /// <summary>
    /// The <c>Func</c> that powers this <c>CLBinaryOperator</c>.
    /// </summary>
    public Func<CalcObject, CalcObject, CLLocalStore, object, CalcValue> Function { get; }

    /// <summary>
    /// Creates a new <c>CLBinaryOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="function">
    /// The backing <c>Func</c> of the operator.
    /// </param>
    public CLBinaryOperator(string symbol, int priority, Func<CalcObject, CalcObject, CLLocalStore, object, CalcValue> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
    }

    /// <summary>Runs the Operator on two operands.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject left, CalcObject right, CLLocalStore vars = null, object context = null) => Function.Invoke(left, right, vars ?? new CLLocalStore(), context);

    public override string ToString() => "bin:" + Symbol;

    /// <summary>
    /// Returns the <c>CLBinaryOperator</c> with the given <c>symbol</c>.
    /// </summary>
    /// <param name="symbol">The symbol for which to check.</param>
    /// <exception cref="KeyNotFoundException">
    /// If there is no <c>CLBinaryOperator</c> with that <c>symbol</c>.
    /// </exception>
    public static CLBinaryOperator Get(string symbol) => (CLBinaryOperator)BinaryOperators[symbol];
  }

  /// <summary>
  /// Represents a unary (one-operand) <c>CLOperator</c>.
  /// </summary>
  public abstract class CLUnaryOperator : CLOperator {
    /// <summary>
    /// The <c>Func</c> that powers this <c>CLUnaryOperator</c>.
    /// </summary>
    public Func<CalcObject, CLLocalStore, object, CalcValue> Function { get; }
    /// <summary>
    /// Whether the operator precedes (<c>true</c>) or follows
    ///   (<c>false</c>) the operand.
    /// </summary>
    public bool IsPrefix { get; }

    internal CLUnaryOperator(string symbol, int priority, bool isPrefix, Func<CalcObject, CLLocalStore, object, CalcValue> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
      IsPrefix = isPrefix;
    }

    /// <summary>Runs the Operator on an operand.</summary>
    /// <param name="operand">The operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject operand, CLLocalStore vars = null, object context = null) => Function.Invoke(operand, vars ?? new CLLocalStore(), context);

    public override string ToString() => (IsPrefix ? "pre:" : "post:") + Symbol;
  }

  /// <summary>
  /// Represents a prefix-based <c>CLUnaryOperator</c>.
  /// </summary>
  public class CLPrefixOperator : CLUnaryOperator {
    /// <summary>
    /// Creates a new <c>CLPrefixOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="function">
    /// The backing <c>Func</c> of the operator.
    /// </param>
    public CLPrefixOperator(string symbol, int priority, Func<CalcObject, CLLocalStore, object, CalcValue> function) : base(symbol, priority, true, function) { }

    /// <summary>
    /// Returns the <c>CLPrefixOperator</c> with the given <c>symbol</c>.
    /// </summary>
    /// <param name="symbol">The symbol for which to check.</param>
    /// <exception cref="KeyNotFoundException">
    /// If there is no <c>CLPrefixOperator</c> with that <c>symbol</c>.
    /// </exception>
    public static CLPrefixOperator Get(string symbol) => (CLPrefixOperator)PrefixOperators[symbol];
  }

  /// <summary>
  /// Represents a postfix-based <c>CLUnaryOperator</c>.
  /// </summary>
  public class CLPostfixOperator : CLUnaryOperator {
    /// <summary>
    /// Creates a new <c>CLPostfixOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="function">
    /// The backing <c>Func</c> of the operator.
    /// </param>
    public CLPostfixOperator(string symbol, int priority, Func<CalcObject, CLLocalStore, object, CalcValue> function) : base(symbol, priority, true, function) { }

    /// <summary>
    /// Returns the <c>CLPostfixOperator</c> with the given <c>symbol</c>.
    /// </summary>
    /// <param name="symbol">The symbol for which to check.</param>
    /// <exception cref="KeyNotFoundException">
    /// If there is no <c>CLPostfixOperator</c> with that <c>symbol</c>.
    /// </exception>
    public static CLPostfixOperator Get(string symbol) => (CLPostfixOperator)PostfixOperators[symbol];
  }
}