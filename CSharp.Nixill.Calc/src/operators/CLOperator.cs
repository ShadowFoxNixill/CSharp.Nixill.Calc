using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Parsing;
using Nixill.CalcLib.Varaibles;
using Nixill.Utils;

namespace Nixill.CalcLib.Operators {
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

    public static readonly CLOperatorList<CLPrefixOperator> PrefixOperators = new CLOperatorList<CLPrefixOperator>();
    public static readonly CLOperatorList<CLBinaryOperator> BinaryOperators = new CLOperatorList<CLBinaryOperator>();
    public static readonly CLOperatorList<CLPostfixOperator> PostfixOperators = new CLOperatorList<CLPostfixOperator>();

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
    private static string KeysToPattern(string[] keys) {
      string ret = "";
      foreach (string key in keys) {
        ret += "|" + rgxSymbol.Replace(key, @"\$1");
      }
      if (ret == "") return "()";
      else return "(" + ret.Substring(1) + ")";
    }

    // Initializes regexes with the existing keys
    private static void InitRegexes() {
      ptnPrefix = KeysToPattern(PrefixOperators.Keys);
      rgxPrefix = new Regex(ptnPrefix);

      ptnPostfix = KeysToPattern(PostfixOperators.Keys);
      rgxPostfix = new Regex(ptnPostfix);

      string ptnBinary = KeysToPattern(BinaryOperators.Keys);
      rgxCombined = new Regex("(" + ptnPostfix + "*)" + ptnBinary + "(" + ptnPrefix + "*)");

      rgxInitiated = true;
    }
  }

  public class CLOperatorList<T> where T : CLOperator {
    private Dictionary<string, T> OperList = new Dictionary<string, T>();

    public T this[string key] {
      get => OperList[key];
      internal set => OperList[key] = value;
    }

    public bool ContainsKey(string key) => OperList.ContainsKey(key);

    public T GetOrNull(string key) {
      if (ContainsKey(key)) return OperList[key];
      else return null;
    }

    public string[] Keys => OperList.Keys.ToArray();
  }

  public abstract class CLOperator {
    internal CLOperator(string symbol, int priority) {
      Priority = priority;
      Symbol = symbol;

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
    /// Returns a string representation of this <c>CLOperator</c>.
    /// </summary>
    public abstract override string ToString();
  }

  /// <summary>
  /// Represents a binary (two-operand) <c>CLOperator</c>.
  /// </summary>
  public class CLBinaryOperator : CLOperator {
    private Dictionary<Type, Dictionary<Type, CLBinaryOperatorFunc>> Functions = new Dictionary<Type, Dictionary<Type, CLBinaryOperatorFunc>>();

    /// <summary>
    /// Iff <c>true</c>, this <c>CLBinaryOperator</c> only deals with
    /// values for its left operand.
    /// </summary>
    public readonly bool ValueBasedLeft = false;

    /// <summary>
    /// Iff <c>true</c>, this <c>CLBinaryOperator</c> only deals with
    /// values for its right operand.
    /// </summary>
    public readonly bool ValueBasedRight = false;

    /// <summary>
    /// Returns the function that'll be run for the given types.
    /// </summary>
    public virtual CLBinaryOperatorFunc this[Type left, Type right] {
      get {
        for (; left != typeof(object); left = left.BaseType) {
          if (Functions.ContainsKey(left)) {
            for (Type r = right; r != typeof(object); r = r.BaseType) {
              if (Functions[left].ContainsKey(r)) return Functions[left][r];
            }
          }
        }

        return null;
      }
    }

    /// <summary>
    /// Creates a new <c>CLBinaryOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="valLeft">
    /// Whether or not the operator is value-based on its left side.
    /// </param>
    /// <param name="valRight">
    /// Whether or not the operator is value-based on its right side.
    /// </param>
    public CLBinaryOperator(string symbol, int priority, bool valLeft, bool valRight) : base(symbol, priority) {
      ValueBasedLeft = valLeft;
      ValueBasedRight = valRight;
    }

    /// <summary>Runs the Operator on two operands.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject left, CalcObject right, CLLocalStore vars = null, CLContextProvider context = null) {
      // If the operator is value-based, we'll automatically convert expressions.
      if (ValueBasedLeft) left = left.GetValue(vars, context);
      if (ValueBasedRight) right = right.GetValue(vars, context);

      // Now get the func.
      CLBinaryOperatorFunc func = this[left.GetType(), right.GetType()];

      // If it's null, we'll throw an exception.
      if (func == null) throw new CLException(
        "Binary operator " + Symbol + " doesn't support parameters " + left.GetType().Name + " and " + right.GetType().Name
      );

      // Now let's run it.
      return func(left, right, vars, context);
    }

    /// <summary>
    /// Adds a new function for the given types.
    /// </summary>
    /// <param name="left">The type of the left operand.</param>
    /// <param name="right">The type of the right operand.</param>
    /// <param name="func">
    /// The function that handles those operands.
    /// </param>
    /// <param name="replaceChildren">
    /// Iff <c>true</c>, the functions that handle types more derived than
    /// <c>left</c> and <c>right</c> should be removed from this operator.
    /// </param>
    public virtual void AddFunction(Type left, Type right, CLBinaryOperatorFunc func, bool replaceChildren = true) {
      Dictionary<Type, CLBinaryOperatorFunc> subDict;

      if (!Functions.ContainsKey(left)) {
        subDict = new Dictionary<Type, CLBinaryOperatorFunc>();
        Functions[left] = subDict;
      }
      else subDict = Functions[left];

      subDict[right] = func;

      // Now replace all the child types if necessary.
      if (replaceChildren) {
        foreach (Type leftTest in Functions.Keys) {
          if (leftTest.IsSubclassOf(left) || leftTest == left) {
            foreach (Type rightTest in Functions[leftTest].Keys) {
              if (rightTest.IsSubclassOf(right) || (rightTest == right && leftTest != left)) {
                Functions[left].Remove(right);
              }
            }
          }
        }
      }
    }

    public override string ToString() => "bin:" + Symbol;
  }

  public delegate CalcValue CLBinaryOperatorFunc(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context);

  /// <summary>
  /// Represents a unary (one-operand) <c>CLOperator</c>.
  /// </summary>
  public class CLUnaryOperator : CLOperator {
    private Dictionary<Type, CLUnaryOperatorFunc> Functions = new Dictionary<Type, CLUnaryOperatorFunc>();

    /// <summary>
    /// Iff <c>true</c>, this <c>CLUnaryOperator</c> only deals with
    /// values.
    /// </summary>
    public readonly bool ValueBased = false;

    /// <summary>
    /// Whether the operator precedes (<c>true</c>) or follows
    ///   (<c>false</c>) the operand.
    /// </summary>
    public bool IsPrefix { get; }

    public CLUnaryOperatorFunc this[Type param] {
      get {
        for (; param != typeof(object); param = param.BaseType) {
          if (Functions.ContainsKey(param)) return Functions[param];
        }

        return null;
      }
    }

    /// <summary>
    /// Creates a new <c>CLUnaryOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="valBased">
    /// Whether or not the operator is value-based.
    /// </param>
    public CLUnaryOperator(string symbol, int priority, bool isPrefix, bool valBased) : base(symbol, priority) {
      IsPrefix = isPrefix;
      ValueBased = valBased;
    }

    /// <summary>Runs the Operator on two operands.</summary>
    /// <param name="param">The right operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject param, CLLocalStore vars = null, CLContextProvider context = null) {
      // If the operator is value-based, we'll automatically convert expressions.
      if (ValueBased) param = param.GetValue(vars, context);

      // Now get the func.
      CLUnaryOperatorFunc func = this[param.GetType()];

      // If it's null, we'll throw an exception.
      if (func == null) throw new CLException(
        "Binary operator " + Symbol + " doesn't support parameter " + param.GetType().Name
      );

      // Now let's run it.
      return func(param, vars, context);
    }

    /// <summary>
    /// Adds a new function for the given types.
    /// </summary>
    /// <param name="param">The type of the operand.</param>
    /// <param name="func">The function that handles this operand.</param>
    /// <param name="replaceChildren">
    /// Iff <c>true</c>, the functions that handle types more derived than
    /// <c>param</c> should be removed from this operator.
    /// </param>
    public void AddFunction(Type param, CLUnaryOperatorFunc func, bool replaceChildren = true) {
      Functions[param] = func;

      // Now replace all the child types if necessary.
      if (replaceChildren) {
        foreach (Type paramTest in Functions.Keys) {
          if (paramTest.IsSubclassOf(param)) {
            Functions.Remove(param);
          }
        }
      }
    }

    public override string ToString() => "bin:" + Symbol;
  }

  public delegate CalcValue CLUnaryOperatorFunc(CalcObject param, CLLocalStore vars, CLContextProvider context);

  /// <summary>
  /// Represents a prefix-based <c>CLUnaryOperator</c>.
  /// </summary>
  public class CLPrefixOperator : CLUnaryOperator {
    /// <summary>
    /// Creates a new <c>CLPrefixOperator</c>.
    /// </summary>
    /// <param name="symbol">The symbol the operator should use.</param>
    /// <param name="priority">The priority of the operator.</param>
    /// <param name="isValueBased">
    /// Whether or not the <c>CLPrefixOperator</c> is value-based.
    /// </param>
    public CLPrefixOperator(string symbol, int priority, bool isValueBased) : base(symbol, priority, true, isValueBased) { }
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
    /// <param name="isValueBased">
    /// Whether or not the <c>CLPostfixOperator</c> is value-based.
    /// </param>
    public CLPostfixOperator(string symbol, int priority, bool isValueBased) : base(symbol, priority, true, isValueBased) { }
  }
}