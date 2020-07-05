using System;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  public class CLComparisonOperatorSet<R> where R : CalcValue {
    public Func<CalcObject, CLComparison, CalcObject, object, CLLocalStore, R> CompFunction { get; }
    public string PrefixSymbol { get; }
    public int Priority { get; }

    public CLComparisonOperator<R> Greater { get; }
    public CLComparisonOperator<R> Equal { get; }
    public CLComparisonOperator<R> Less { get; }
    public CLComparisonOperator<R> NotGreater { get; }
    public CLComparisonOperator<R> NotEqual { get; }
    public CLComparisonOperator<R> NotLess { get; }
    public CLComparisonOperator<R> Modulo { get; }
    public CLComparisonOperator<R> NotModulo { get; }

    public CLComparisonOperatorSet(string prefix, int priority, Func<CalcObject, CLComparison, CalcObject, object, CLLocalStore, R> compFunction) {
      PrefixSymbol = prefix;
      Priority = priority;
      CompFunction = compFunction;

      Greater = new CLComparisonOperator<R>(this, CLComparison.Greater);
      Equal = new CLComparisonOperator<R>(this, CLComparison.Equal);
      Less = new CLComparisonOperator<R>(this, CLComparison.Less);
      NotGreater = new CLComparisonOperator<R>(this, CLComparison.NotGreater);
      NotEqual = new CLComparisonOperator<R>(this, CLComparison.NotEqual);
      NotLess = new CLComparisonOperator<R>(this, CLComparison.NotLess);
      Modulo = new CLComparisonOperator<R>(this, CLComparison.Modulo);
      NotModulo = new CLComparisonOperator<R>(this, CLComparison.NotModulo);
    }
  }

  public class CLComparison {
    public string PostfixSymbol;
    public Func<decimal, decimal, bool> CompareFunction;

    private CLComparison(string symbol, Func<decimal, decimal, bool> func) {
      PostfixSymbol = symbol;
      CompareFunction = func;
    }

    public static readonly CLComparison Greater = new CLComparison(">", (left, right) => left > right);
    public static readonly CLComparison Equal = new CLComparison("=", (left, right) => left == right);
    public static readonly CLComparison Less = new CLComparison("<", (left, right) => left < right);
    public static readonly CLComparison NotGreater = new CLComparison("<=", (left, right) => left <= right);
    public static readonly CLComparison NotEqual = new CLComparison("!=", (left, right) => left != right);
    public static readonly CLComparison NotLess = new CLComparison(">=", (left, right) => left >= right);
    public static readonly CLComparison Modulo = new CLComparison("%", (left, right) => left % right == 0);
    public static readonly CLComparison NotModulo = new CLComparison("!%", (left, right) => left % right != 0);
  }

  public class CLComparisonOperator<R> : CLBinaryOperator<R> where R : CalcValue {
    internal CLComparisonOperator(CLComparisonOperatorSet<R> set, CLComparison comp) :
      base(set.PrefixSymbol + comp.PostfixSymbol, set.Priority, (left, right, context, vars) => set.CompFunction(left, comp, right, context, vars)) { }
  }
}