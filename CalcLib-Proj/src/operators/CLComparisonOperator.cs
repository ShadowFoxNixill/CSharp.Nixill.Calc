using System;
using System.Collections;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  /// <summary>
  /// Provides a set of eight <c>CLComparisonOperator</c>s that share a
  ///   common function.
  /// </summary>
  public class CLComparisonOperatorSet : IEnumerable<CLComparisonOperator> {
    private Dictionary<Type, Dictionary<Type, CLComparisonFunc>> Functions = new Dictionary<Type, Dictionary<Type, CLComparisonFunc>>();

    /// <summary>
    /// Returns the function that'll be run for the given types.
    /// </summary>
    public CLComparisonFunc this[Type left, Type right] {
      get {
        for (; left != typeof(object); left = left.BaseType) {
          if (Functions.ContainsKey(left)) {
            for (Type r = right; r != typeof(object); r = r.BaseType) {
              if (Functions.ContainsKey(right)) return Functions[left][right];
            }
          }
        }

        return null;
      }
    }

    public string PrefixSymbol { get; }
    public int Priority { get; }
    public bool ValueBasedLeft { get; }
    public bool ValueBasedRight { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Greater Than.
    /// </summary>+
    public CLComparisonOperator Greater { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Equal To.
    /// </summary>
    public CLComparisonOperator Equal { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Less Than.
    /// </summary>
    public CLComparisonOperator Less { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Less Than or Equal To.
    /// </summary>
    public CLComparisonOperator NotGreater { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Not Equal To.
    /// </summary>
    public CLComparisonOperator NotEqual { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Greater Than or Equal To.
    /// </summary>
    public CLComparisonOperator NotLess { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Multiple Of (Modulo).
    /// </summary>
    public CLComparisonOperator Modulo { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Not Multiple Of (Modulo).
    /// </summary>
    public CLComparisonOperator NotModulo { get; }

    public IEnumerator<CLComparisonOperator> GetEnumerator() {
      yield return Greater;
      yield return Equal;
      yield return Less;
      yield return NotGreater;
      yield return NotEqual;
      yield return NotLess;
      yield return Modulo;
      yield return NotModulo;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    /// <summary>
    /// Creates a new <c>CLComparisonOperatorSet</c>.
    /// </summary>
    /// <param name="prefix">
    /// The symbol prefix to be put before the comparison in the overall
    ///   symbol.
    /// </param>
    /// <param name="priority">The priority of the operators.</param>
    /// <param name="compFunction">The function that runs.</param>
    /// <param name="valLeft">
    /// Whether or not these <c>CLComparisonOperators</c> are value-based
    /// on their left sides.
    /// </param>
    /// <param name="valRight">
    /// Whether or not these <c>CLComparisonOperators</c> are value-based
    /// on their right sides.
    /// </param>
    public CLComparisonOperatorSet(string prefix, int priority, bool valLeft, bool valRight) {
      PrefixSymbol = prefix;
      Priority = priority;
      ValueBasedLeft = valLeft;
      ValueBasedRight = valRight;

      Greater = new CLComparisonOperator(this, CLComparison.Greater);
      Equal = new CLComparisonOperator(this, CLComparison.Equal);
      Less = new CLComparisonOperator(this, CLComparison.Less);
      NotGreater = new CLComparisonOperator(this, CLComparison.NotGreater);
      NotEqual = new CLComparisonOperator(this, CLComparison.NotEqual);
      NotLess = new CLComparisonOperator(this, CLComparison.NotLess);
      Modulo = new CLComparisonOperator(this, CLComparison.Modulo);
      NotModulo = new CLComparisonOperator(this, CLComparison.NotModulo);
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
    public virtual void AddFunction(Type left, Type right, CLComparisonFunc func, bool replaceChildren = true) {
      Dictionary<Type, CLComparisonFunc> subDict;

      if (!Functions.ContainsKey(left)) {
        subDict = new Dictionary<Type, CLComparisonFunc>();
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
  }

  public delegate CalcValue CLComparisonFunc(CalcObject left, CLComparison comp, CalcObject right, CLLocalStore vars, CLContextProvider context);

  /// <summary>
  /// A single comparison, with its own symbol and comparison condition.
  /// </summary>
  public class CLComparison : IEnumerable<CLComparison> {
    /// <summary>The symbol for the comparison.</summary>
    public string PostfixSymbol { get; }
    /// <summary>
    /// A function that takes two numbers and returns whether one compares
    ///   to the other in the specified manner.
    /// </summary>
    public Func<decimal, decimal, bool> CompareFunction { get; }

    private CLComparison(string symbol, Func<decimal, decimal, bool> func) {
      PostfixSymbol = symbol;
      CompareFunction = func;
    }

    /// <summary>
    /// Returns an iterator over all <c>CLComparison</c>s.
    /// </summary>
    public IEnumerator<CLComparison> GetEnumerator() {
      yield return Greater;
      yield return Equal;
      yield return Less;
      yield return NotGreater;
      yield return NotEqual;
      yield return NotLess;
      yield return Modulo;
      yield return NotModulo;
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    /// <summary>
    /// The "greater than" comparison, returning true iff
    ///   <c>left > right</c>.
    /// </summary>
    public static readonly CLComparison Greater = new CLComparison(">", (left, right) => left > right);

    /// <summary>
    /// The "equal to" comparison, returning true iff
    ///   <c>left == right</c>.
    /// </summary>
    public static readonly CLComparison Equal = new CLComparison("=", (left, right) => left == right);

    /// <summary>
    /// The "less than" comparison, returning true iff
    ///   <c>left < right</c>.
    /// </summary>
    public static readonly CLComparison Less = new CLComparison("<", (left, right) => left < right);

    /// <summary>
    /// The "not greater than" or "less than or equal to" comparison,
    ///   returning true iff <c>left <= right</c>.
    /// </summary>
    public static readonly CLComparison NotGreater = new CLComparison("<=", (left, right) => left <= right);

    /// <summary>
    /// The "not equal to" comparison, returning true iff
    ///   <c>left != right</c>.
    /// </summary>
    public static readonly CLComparison NotEqual = new CLComparison("!=", (left, right) => left != right);

    /// <summary>
    /// The "not less than" or "greater than or equal to" comparison,
    ///   returning true iff <c>left >= right</c>.
    /// </summary>
    public static readonly CLComparison NotLess = new CLComparison(">=", (left, right) => left >= right);

    /// <summary>
    /// The "multiple of" or "modulo zero" comparison, returning true iff
    ///   <c>left % right == 0</c>.
    /// </summary>
    public static readonly CLComparison Modulo = new CLComparison("%", (left, right) => left % right == 0);

    /// <summary>
    /// The "not multiple of" or "not modulo zero" comparison, returning
    ///   true iff <c>left % right != 0</c>.
    /// </summary>
    public static readonly CLComparison NotModulo = new CLComparison("!%", (left, right) => left % right != 0);
  }

  /// <summary>
  /// A single <c>CLBinaryOperator</c> created from a
  ///   <c>CLComparisonOperatorSet</c>.
  /// </summary>
  /// <seealso cref="CLBinaryOperator"/>
  /// <seealso cref="CLComparisonOperatorSet"/>
  public class CLComparisonOperator : CLBinaryOperator {
    /// <summary>
    /// The <c>CLComparisonOperatorSet</c> of which this
    /// <c>CLComparisonOperator</c> is a part.
    /// </summary>
    public CLComparisonOperatorSet Parent { get; internal set; }

    /// <summary>
    /// The <c>CLComparison</c> that this <c>CLComparisonOperator</c>
    /// uses in its <c>CLComparisonFunc</c>.
    /// </summary>
    public CLComparison Comparison { get; internal set; }

    public override CLBinaryOperatorFunc this[Type left, Type right] {
      get {
        CLComparisonFunc func = Parent[left, right];

        if (func != null) return (l, r, vars, context) => func(l, Comparison, r, vars, context);
        else return null;
      }
    }

    internal CLComparisonOperator(CLComparisonOperatorSet set, CLComparison comp) :
      base(set.PrefixSymbol + comp.PostfixSymbol, set.Priority, set.ValueBasedLeft, set.ValueBasedRight) {
      Parent = set;
      Comparison = comp;
    }

    /// <summary>
    /// Throws an <c>InvalidOperationException</c>. To add a function, go
    /// through the <c>Parent</c> instead.
    /// </summary>
    public override void AddFunction(Type left, Type right, CLBinaryOperatorFunc func, bool replaceChildren) {
      throw new InvalidOperationException("For a CLComparisonOperator, functions must be added through the set.");
    }
  }
}