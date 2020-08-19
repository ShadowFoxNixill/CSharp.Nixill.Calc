using System;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Functions;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;
using Nixill.CalcLib.Varaibles;
using static Nixill.CalcLib.Modules.Casting;

namespace Nixill.CalcLib.Modules {
  public class MathModule {
    public static bool Loaded { get; private set; } = false;

    private static CalcNumber numE;
    private static CalcNumber numPI;

    public static CLCodeFunction E { get; private set; }
    public static CLCodeFunction PI { get; private set; }

    public static CLCodeFunction Abs { get; private set; }
    public static CLCodeFunction Acos { get; private set; }
    public static CLCodeFunction Acosh { get; private set; }
    public static CLCodeFunction Asin { get; private set; }
    public static CLCodeFunction Asinh { get; private set; }
    public static CLCodeFunction Atan { get; private set; }
    public static CLCodeFunction Atan2 { get; private set; }
    public static CLCodeFunction Atanh { get; private set; }
    public static CLCodeFunction Ceiling { get; private set; }
    public static CLCodeFunction CopySign { get; private set; }
    public static CLCodeFunction Cos { get; private set; }
    public static CLCodeFunction Cosh { get; private set; }
    public static CLCodeFunction Floor { get; private set; }
    public static CLCodeFunction Max { get; private set; }
    public static CLCodeFunction MaxMagnitude { get; private set; }
    public static CLCodeFunction Median { get; private set; }
    public static CLCodeFunction MedianMagnitude { get; private set; }
    public static CLCodeFunction Min { get; private set; }
    public static CLCodeFunction MinMagnitude { get; private set; }
    public static CLCodeFunction Sign { get; private set; }
    public static CLCodeFunction Sin { get; private set; }
    public static CLCodeFunction Sinh { get; private set; }
    public static CLCodeFunction Tan { get; private set; }
    public static CLCodeFunction Tanh { get; private set; }

    public static CLBinaryOperator Exponent { get; private set; }

    public static void Load() {
      // First we need some local types
      Type num = typeof(CalcNumber);
      Type lst = typeof(CalcList);
      Type str = typeof(CalcString);
      Type val = typeof(CalcValue);

      Exponent = CLOperators.BinaryOperators.GetOrNull("^");
      Exponent.AddFunction(num, num, BinPowerNumbers);
      Exponent.AddFunction(lst, num, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), right, vars, context));
      Exponent.AddFunction(lst, lst, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), ListToNum(right), vars, context));
      Exponent.AddFunction(num, lst, (left, right, vars, context) => BinPowerNumbers(left, ListToNum(right), vars, context));

      numE = new CalcNumber((decimal)Math.E);
      numPI = new CalcNumber((decimal)Math.PI);

      E = new CLCodeFunction("e", (pars, vars, context) => numE);
      PI = new CLCodeFunction("pi", (pars, vars, context) => numPI);

      Abs = new CLCodeFunction("abs", AbsFunction);
    }

    // Raises one number to the power of another.
    private static CalcValue BinPowerNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber((decimal)Math.Pow((double)numLeft.Value, (double)numRight.Value));
    }

    private static CalcValue AbsFunction(CalcObject[] pars, CLLocalStore vars, object context) {
      if (pars.Length == 0) throw new CLException("{!abs} requires a number.");

      CalcValue par0 = pars[0].GetValue(vars, context);

      CalcNum 
    }
  }
}