using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Parsing;
using Nixill.CalcLib.Varaibles;
using Nixill.Utils;

namespace Nixill.CalcLib.Operators
{
  public static class CLOperators
  {
    private static HashSet<int> FromRightOperators = new HashSet<int>();

    /// <summary>
    /// Sets a given priority level to group right-to-left.
    /// </summary>
    /// <param name="level">The level to set.</param>
    public static void SetFromRight(int level)
    {
      FromRightOperators.Add(level);
    }

    /// <summary>
    /// Sets a given priority level to group left-to-right.
    /// </summary>
    /// <param name="level">The level to set.</param>
    public static void UnsetFromRight(int level)
    {
      FromRightOperators.Remove(level);
    }

    /// <summary>
    /// Checks whether a given priority level groups right-to-left.
    /// </summary>
    /// <param name="level">The level to check.</param>
    public static bool IsFromRight(int level) => FromRightOperators.Contains(level);

    public static readonly CLOperatorList<CLPrefixOperator> PrefixOperators = new CLOperatorList<CLPrefixOperator>();
    public static readonly CLOperatorList<CLBinaryOperator> BinaryOperators = new CLOperatorList<CLBinaryOperator>();
    public static readonly CLOperatorList<CLPostfixOperator> PostfixOperators = new CLOperatorList<CLPostfixOperator>();

    private static Regex rgxSymbol = new Regex(@"([^a-zA-Z0-9])");
    internal static bool rgxInitiated = false;

    public static IEnumerable<CLObjectPiece> GetOpers(string input, bool prefix, bool postfix, int pos)
    {
      // Remove whitespace
      input = CLLexer.rgxWhitespaceReplace.Replace(input, "");

      // Otherwise let's go through the group to see if we can split it into many
      if (prefix)
      {
        return RecursiveGetPrefixOpers(input, pos);
      }
      else if (postfix)
      {
        return RecursiveGetPostfixOpers(input, pos);
      }
      else
      {
        return RecursiveGetOpers(input, pos);
      }
    }

    // Recursively gets prefix operators from the list
    private static IEnumerable<CLObjectPiece> RecursiveGetPrefixOpers(string input, int pos)
    {
      if (PrefixOperators.ContainsKey(input))
      {
        yield return new CLObjectPiece(input, CLObjectPieceType.PrefixOperator, pos);
        yield break;
      }

      for (int i = 1; i < input.Length; i++)
      {
        string thisOper = input[0..i];
        string remOper = input[i..^0];

        if (PrefixOperators.ContainsKey(thisOper))
        {
          yield return new CLObjectPiece(thisOper, CLObjectPieceType.PrefixOperator, pos);
          foreach (var item in RecursiveGetPrefixOpers(remOper, pos + thisOper.Length))
            yield return item;

          yield break;
        }
      }

      throw new CLSyntaxException($"Unrecognized operator(s): {input}", pos);
    }

    // Recursively gets prefix operators from the list
    private static IEnumerable<CLObjectPiece> RecursiveGetOpers(string input, int pos)
    {
      if (BinaryOperators.ContainsKey(input))
        yield return new CLObjectPiece(input, CLObjectPieceType.BinaryOperator, pos);

      for (int i = 1; i < input.Length; i++)
      {
        string thisOper = input[0..i];
        string remOper = input[i..^0];

        if (BinaryOperators.ContainsKey(thisOper))
        {
          yield return new CLObjectPiece(thisOper, CLObjectPieceType.BinaryOperator, pos);

          foreach (var piece in RecursiveGetPrefixOpers(remOper, pos + thisOper.Length))
            yield return piece;

          yield break;
        }

        else if (PostfixOperators.ContainsKey(thisOper))
        {
          yield return new CLObjectPiece(thisOper, CLObjectPieceType.PostfixOperator, pos);

          foreach (var piece in RecursiveGetOpers(remOper, pos + thisOper.Length))
            yield return piece;

          yield break;
        }
      }

      throw new CLSyntaxException($"Unrecognized operator(s): {input}", pos);
    }

    // Recursively gets prefix operators from the list
    private static IEnumerable<CLObjectPiece> RecursiveGetPostfixOpers(string input, int pos)
    {
      if (PostfixOperators.ContainsKey(input))
        yield return new CLObjectPiece(input, CLObjectPieceType.PostfixOperator, pos);

      for (int i = 1; i < input.Length; i++)
      {
        string thisOper = input[0..i];
        string remOper = input[i..^0];

        if (PostfixOperators.ContainsKey(thisOper))
        {
          yield return new CLObjectPiece(thisOper, CLObjectPieceType.PostfixOperator, pos);

          foreach (var piece in RecursiveGetPostfixOpers(remOper, pos + thisOper.Length))
            yield return piece;

          yield break;
        }
      }

      throw new CLSyntaxException($"Unrecognized operator(s): {input}", pos);
    }

    // Takes the keys from a dictionary and turns them into a regex that matches any one key
    private static string KeysToPattern(string[] keys)
    {
      string ret = "";
      foreach (string key in keys)
      {
        ret += "|" + rgxSymbol.Replace(key, @"\$1");
      }
      if (ret == "") return "()";
      else return "(" + ret.Substring(1) + ")";
    }
  }

  public class CLOperatorList<T> where T : CLOperator
  {
    private Dictionary<string, T> OperList = new Dictionary<string, T>();

    public T this[string key]
    {
      get => OperList[key];
      internal set => OperList[key] = value;
    }

    public bool ContainsKey(string key) => OperList.ContainsKey(key);

    public T GetOrNull(string key)
    {
      if (ContainsKey(key)) return OperList[key];
      else return null;
    }

    public string[] Keys => OperList.Keys.ToArray();
  }

  public abstract class CLOperator
  {
    internal CLOperator(string symbol, int priority)
    {
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
  public class CLBinaryOperator : CLOperator
  {
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
    public virtual CLBinaryOperatorFunc this[Type left, Type right]
    {
      get
      {
        for (; left != typeof(object); left = left.BaseType)
        {
          if (Functions.ContainsKey(left))
          {
            for (Type r = right; r != typeof(object); r = r.BaseType)
            {
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
    public CLBinaryOperator(string symbol, int priority, bool valLeft, bool valRight) : base(symbol, priority)
    {
      ValueBasedLeft = valLeft;
      ValueBasedRight = valRight;
    }

    /// <summary>Runs the Operator on two operands.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject left, CalcObject right, CLLocalStore vars = null, CLContextProvider context = null)
    {
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
    public virtual void AddFunction(Type left, Type right, CLBinaryOperatorFunc func, bool replaceChildren = true)
    {
      Dictionary<Type, CLBinaryOperatorFunc> subDict;

      if (!Functions.ContainsKey(left))
      {
        subDict = new Dictionary<Type, CLBinaryOperatorFunc>();
        Functions[left] = subDict;
      }
      else subDict = Functions[left];

      subDict[right] = func;

      // Now replace all the child types if necessary.
      if (replaceChildren)
      {
        foreach (Type leftTest in Functions.Keys)
        {
          if (leftTest.IsSubclassOf(left) || leftTest == left)
          {
            foreach (Type rightTest in Functions[leftTest].Keys)
            {
              if (rightTest.IsSubclassOf(right) || (rightTest == right && leftTest != left))
              {
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
  public class CLUnaryOperator : CLOperator
  {
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

    public CLUnaryOperatorFunc this[Type param]
    {
      get
      {
        for (; param != typeof(object); param = param.BaseType)
        {
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
    public CLUnaryOperator(string symbol, int priority, bool isPrefix, bool valBased) : base(symbol, priority)
    {
      IsPrefix = isPrefix;
      ValueBased = valBased;
    }

    /// <summary>Runs the Operator on two operands.</summary>
    /// <param name="param">The right operand.</param>
    /// <param name="vars">The local variable storage.</param>
    /// <param name="context">An object representing context.</param>
    public CalcValue Run(CalcObject param, CLLocalStore vars = null, CLContextProvider context = null)
    {
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
    public void AddFunction(Type param, CLUnaryOperatorFunc func, bool replaceChildren = true)
    {
      Functions[param] = func;

      // Now replace all the child types if necessary.
      if (replaceChildren)
      {
        foreach (Type paramTest in Functions.Keys)
        {
          if (paramTest.IsSubclassOf(param))
          {
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
  public class CLPrefixOperator : CLUnaryOperator
  {
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
  public class CLPostfixOperator : CLUnaryOperator
  {
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