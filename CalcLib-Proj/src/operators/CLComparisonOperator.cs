using System;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Operators {
  public class CLComparisonOperatorSet {
    public Func<CalcObject, CLComparison, CalcObject, CLLocalStore, object, CalcValue> CompFunction { get; }
    public string PrefixSymbol { get; }
    public int Priority { get; }

    public CLComparisonOperator Greater { get; }
    public CLComparisonOperator Equal { get; }
    public CLComparisonOperator Less { get; }
    public CLComparisonOperator NotGreater { get; }
    public CLComparisonOperator NotEqual { get; }
    public CLComparisonOperator NotLess { get; }
    public CLComparisonOperator Modulo { get; }
    public CLComparisonOperator NotModulo { get; }

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

  public class CLComparisonOperator : CLBinaryOperator {
    internal CLComparisonOperator(CLComparisonOperatorSet set, CLComparison comp) :
      base(set.PrefixSymbol + comp.PostfixSymbol, set.Priority, (left, right, vars, context) => set.CompFunction(left, comp, right, vars, context)) { }
  }
}