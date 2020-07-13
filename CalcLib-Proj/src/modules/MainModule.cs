using System;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Modules {
  public class MainModule {
    public static bool Loaded { get; private set; } = false;

    public const int PlusPriority = 0;
    public const int TimesPriority = PlusPriority + 5;

    public static CLBinaryOperator BinaryPlus { get; private set; }
    public static CLBinaryOperator BinaryMinus { get; private set; }
    public static CLBinaryOperator BinaryTimes { get; private set; }
    public static CLBinaryOperator BinaryDivide { get; private set; }
    public static CLBinaryOperator BinaryIntDivide { get; private set; }
    public static CLBinaryOperator BinaryModulo { get; private set; }

    public static CLPrefixOperator PrefixMinus { get; private set; }

    public static void Load() {
      // First we need some local calcvalues
      Type num = typeof(CalcNumber);
      Type lst = typeof(CalcList);
      Type str = typeof(CalcString);
      Type val = typeof(CalcValue);

      // The binary + operator
      BinaryPlus = CLOperators.BinaryOperators.GetOrNull("+") ?? new CLBinaryOperator("+", PlusPriority, true, true);

      BinaryPlus.AddFunction(num, num, BinPlusNumbers, true);
      BinaryPlus.AddFunction(num, str, (left, right, vars, context) => BinPlusStrings(NumToString(left), right, vars, context), true);
      BinaryPlus.AddFunction(str, num, (left, right, vars, context) => BinPlusStrings(left, NumToString(right), vars, context), true);
      BinaryPlus.AddFunction(str, str, BinPlusStrings, true);
      BinaryPlus.AddFunction(val, lst, (left, right, vars, context) => BinPlusLists(ValToList(left), right, vars, context), true);
      BinaryPlus.AddFunction(lst, val, (left, right, vars, context) => BinPlusLists(left, ValToList(right), vars, context), true);
      BinaryPlus.AddFunction(lst, lst, BinPlusLists, true);

      BinaryMinus = CLOperators.BinaryOperators.GetOrNull("-") ?? new CLBinaryOperator("-", PlusPriority, true, true);
      BinaryTimes = CLOperators.BinaryOperators.GetOrNull("*") ?? new CLBinaryOperator("*", TimesPriority, true, true);
      BinaryDivide = CLOperators.BinaryOperators.GetOrNull("/") ?? new CLBinaryOperator("/", TimesPriority, true, true);
      BinaryIntDivide = CLOperators.BinaryOperators.GetOrNull("//") ?? new CLBinaryOperator("//", TimesPriority, true, true);
      BinaryModulo = CLOperators.BinaryOperators.GetOrNull("%") ?? new CLBinaryOperator("%", TimesPriority, true, true);

      PrefixMinus = CLOperators.PrefixOperators.GetOrNull("-") ?? new CLPrefixOperator("-", PlusPriority, true);
    }

    private static CalcString NumToString(CalcObject num) => new CalcString((num as CalcNumber).Value.ToString("0.#####"));
    private static CalcList ValToList(CalcObject val) => new CalcList(new CalcValue[] { (val as CalcValue) });

    private static CalcValue BinPlusNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft + numRight);
    }

    private static CalcValue BinPlusStrings(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcString strLeft = left as CalcString;
      CalcString strRight = right as CalcString;

      return new CalcString(strLeft + strRight);
    }

    private static CalcValue BinPlusLists(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcList lstLeft = left as CalcList;
      CalcList lstRight = right as CalcList;

      CalcValue[] lstRet = new CalcValue[lstLeft.Count + lstRight.Count];

      int i;

      for (i = 0; i < lstLeft.Count; i++) {
        lstRet[i] = lstLeft[i];
      }

      for (int j = 0; j < lstRight.Count; j++) {
        lstRet[i + j] = lstRight[j];
      }

      return new CalcList(lstRet);
    }
  }
}