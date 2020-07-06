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

  public class CLBinaryOperator : CLOperator {
    public Func<CalcObject, CalcObject, CLLocalStore, object, CalcValue> Function { get; }

    public CLBinaryOperator(string symbol, int priority, Func<CalcObject, CalcObject, CLLocalStore, object, CalcValue> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
    }

    public CalcValue Run(CalcObject left, CalcObject right, CLLocalStore vars = null, object context = null) => Function.Invoke(left, right, vars, context);

    public override string ToString() => "bin:" + Symbol;
  }

  public abstract class CLUnaryOperator : CLOperator {
    public Func<CalcObject, CLLocalStore, object, CalcValue> Function { get; }
    public bool IsPrefix { get; }

    internal CLUnaryOperator(string symbol, int priority, bool isPrefix, Func<CalcObject, CLLocalStore, object, CalcValue> function) {
      Symbol = symbol;
      Priority = priority;
      Function = function;
      IsPrefix = isPrefix;
    }

    public CalcValue Run(CalcObject operand, CLLocalStore vars = null, object context = null) => Function.Invoke(operand, vars, context);

    public override string ToString() => (IsPrefix ? "pre:" : "post:") + Symbol;
  }

  public class CLPrefixOperator : CLUnaryOperator {
    public CLPrefixOperator(string symbol, int priority, Func<CalcObject, CLLocalStore, object, CalcValue> function) : base(symbol, priority, true, function) { }
  }

  public class CLPostfixOperator : CLUnaryOperator {
    public CLPostfixOperator(string symbol, int priority, Func<CalcObject, CLLocalStore, object, CalcValue> function) : base(symbol, priority, true, function) { }
  }
}