using System;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  /// <summary>
  /// Represents an operator within an expression.
  /// </summary>
  public abstract class CLOperator {
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



    internal CLOperator() { }

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
  }
}