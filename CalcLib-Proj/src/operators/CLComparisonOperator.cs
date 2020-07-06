using System;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  /// <summary>
  /// Provides a set of eight <c>CLComparisonOperator</c>s that share a
  ///   common function.
  /// </summary>
  public class CLComparisonOperatorSet {
    public Func<CalcObject, CLComparison, CalcObject, CLLocalStore, object, CalcValue> CompFunction { get; }
    public string PrefixSymbol { get; }
    public int Priority { get; }

    /// <summary>
    /// The <c>CLComparisonOperator</c> for Greater Than.
    /// </summary>
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

    /// <summary>
    /// Creates a new <c>CLComparisonOperatorSet</c>.
    /// </summary>
    /// <param name="prefix">
    /// The symbol prefix to be put before the comparison in the overall
    ///   symbol.
    /// </param>
    /// <param name="priority">The priority of the operators.</param>
    /// <param name="compFunction">The function that runs.</param>
    public CLComparisonOperatorSet(string prefix, int priority, Func<CalcObject, CLComparison, CalcObject, CLLocalStore, object, CalcValue> compFunction) {
      PrefixSymbol = prefix;
      Priority = priority;
      CompFunction = compFunction;

      Greater = new CLComparisonOperator(this, CLComparison.Greater);
      Equal = new CLComparisonOperator(this, CLComparison.Equal);
      Less = new CLComparisonOperator(this, CLComparison.Less);
      NotGreater = new CLComparisonOperator(this, CLComparison.NotGreater);
      NotEqual = new CLComparisonOperator(this, CLComparison.NotEqual);
      NotLess = new CLComparisonOperator(this, CLComparison.NotLess);
      Modulo = new CLComparisonOperator(this, CLComparison.Modulo);
      NotModulo = new CLComparisonOperator(this, CLComparison.NotModulo);
    }
  }

  /// <summary>
  /// A single comparison, with its own symbol and comparison condition.
  /// </summary>
  public class CLComparison {
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
    internal CLComparisonOperator(CLComparisonOperatorSet set, CLComparison comp) :
      base(set.PrefixSymbol + comp.PostfixSymbol, set.Priority, (left, right, vars, context) => set.CompFunction(left, comp, right, vars, context)) { }
  }
}