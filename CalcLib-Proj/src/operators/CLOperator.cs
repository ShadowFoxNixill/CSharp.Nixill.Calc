using System;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  public abstract class CLOperator {
    private static HashSet<int> FromRightOperators = new HashSet<int>();

    public static void SetFromRight(int level) {
      FromRightOperators.Add(level);
    }

    public static void UnsetFromRight(int level) {
      FromRightOperators.Remove(level);
    }

    public static bool IsFromRight(int level) => FromRightOperators.Contains(level);

    internal CLOperator() { }

    public int Priority { get; protected set; }
    public string Symbol { get; protected set; }

    public abstract override string ToString();
  }

  public class CLBinaryOperator<R> : CLOperator where R : CalcValue {
    public Func<CalcObject, CalcObject, object, CLLocalStore, R> Function { get; }

    public CLBinaryOperator(string symbol, int priority, Func<CalcObject, CalcObject, object, CLLocalStore, R> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
    }

    public R Run(CalcObject left, CalcObject right, object context = null, CLLocalStore vars = null) => Function.Invoke(left, right, context, vars);

    public override string ToString() => "bin:" + Symbol;
  }

  public abstract class CLUnaryOperator<R> : CLOperator where R : CalcValue {
    public Func<CalcObject, object, CLLocalStore, R> Function { get; }
    public bool IsPrefix { get; }

    internal CLUnaryOperator(string symbol, int priority, bool isPrefix, Func<CalcObject, object, CLLocalStore, R> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
      IsPrefix = isPrefix;
    }

    public R Run(CalcObject operand, object context = null, CLLocalStore vars = null) => Function.Invoke(operand, context, vars);

    public override string ToString() => (IsPrefix ? "pre:" : "post:") + Symbol;
  }

  public class CLPrefixOperator<R> : CLUnaryOperator<R> where R : CalcValue {
    public CLPrefixOperator(string symbol, int priority, Func<CalcObject, object, CLLocalStore, R> function) : base(symbol, priority, true, function) { }
  }

  public class CLPostfixOperator<R> : CLUnaryOperator<R> where R : CalcValue {
    public CLPostfixOperator(string symbol, int priority, Func<CalcObject, object, CLLocalStore, R> function) : base(symbol, priority, true, function) { }
  }
}