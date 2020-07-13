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

      BinaryPlus.AddFunction(num, num, BinPlusNumbers);
      BinaryPlus.AddFunction(num, str, (left, right, vars, context) => BinPlusStrings(NumToString(left), right, vars, context));
      BinaryPlus.AddFunction(str, num, (left, right, vars, context) => BinPlusStrings(left, NumToString(right), vars, context));
      BinaryPlus.AddFunction(str, str, BinPlusStrings);
      BinaryPlus.AddFunction(val, lst, (left, right, vars, context) => BinPlusLists(ValToList(left), right, vars, context));
      BinaryPlus.AddFunction(lst, val, (left, right, vars, context) => BinPlusLists(left, ValToList(right), vars, context));
      BinaryPlus.AddFunction(lst, lst, BinPlusLists);

      // The binary - operator
      BinaryMinus = CLOperators.BinaryOperators.GetOrNull("-") ?? new CLBinaryOperator("-", PlusPriority, true, true);

      BinaryMinus.AddFunction(val, val, BinMinus);

      // The binary * operator
      BinaryTimes = CLOperators.BinaryOperators.GetOrNull("*") ?? new CLBinaryOperator("*", TimesPriority, true, true);

      BinaryTimes.AddFunction(num, num, BinTimesNumbers);
      BinaryTimes.AddFunction(num, lst, (left, right, vars, context) => BinTimesNumList(right, left, vars, context));
      BinaryTimes.AddFunction(num, str, (left, right, vars, context) => BinTimesNumString(right, left, vars, context));
      BinaryTimes.AddFunction(lst, lst, (left, right, vars, context) => BinTimesNumList(left, ListToNum(right), vars, context));
      BinaryTimes.AddFunction(lst, num, BinTimesNumList);
      BinaryTimes.AddFunction(str, num, BinTimesNumString);

      BinaryDivide = CLOperators.BinaryOperators.GetOrNull("/") ?? new CLBinaryOperator("/", TimesPriority, true, true);
      BinaryIntDivide = CLOperators.BinaryOperators.GetOrNull("//") ?? new CLBinaryOperator("//", TimesPriority, true, true);
      BinaryModulo = CLOperators.BinaryOperators.GetOrNull("%") ?? new CLBinaryOperator("%", TimesPriority, true, true);

      PrefixMinus = CLOperators.PrefixOperators.GetOrNull("-") ?? new CLPrefixOperator("-", PlusPriority, true);
    }

    // Converts a number to a string.
    private static CalcString NumToString(CalcObject num) => new CalcString((num as CalcNumber).Value.ToString("0.#####"));

    // Converts a value to a list.
    private static CalcList ValToList(CalcObject val) => new CalcList(new CalcValue[] { (val as CalcValue) });

    // Converts a list to a number.
    private static CalcNumber ListToNum(CalcObject lst) => new CalcNumber((lst as CalcList).Sum());

    #region // BIN + FUNCTIONS //
    // Adds two numbers.
    private static CalcValue BinPlusNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft + numRight);
    }

    // Concatenates two strings.
    private static CalcValue BinPlusStrings(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcString strLeft = left as CalcString;
      CalcString strRight = right as CalcString;

      return new CalcString(strLeft + strRight);
    }

    // Concatenates two lists.
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
    #endregion

    #region // BIN - FUNCTION //
    // Subtracts one value from another.
    private static CalcValue BinMinus(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CLBinaryOperator binPlus = CLOperators.BinaryOperators["+"];
      CLPrefixOperator preMinus = CLOperators.PrefixOperators["-"];

      binPlus.Run(left, preMinus.Run(right, vars, context), vars, context);
    }
    #endregion

    #region // BIN * FUNCTIONS //
    // Multiples two numbers and returns their product.
    private CalcValue BinTimesNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft * numRight);
    }

    // Multiplies a list by a number.
    private CalcValue BinTimesNumList(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcList lstLeft = left as CalcList;
      CalcNumber numRight = right as CalcNumber;

      // If it has a string, we'll just repeat the list multiple times.
      if (lstLeft.HasString()) {
        int count = (int)numRight;
        if (count < 0) throw new CLException("Lists cannot be repeated negative times.");

        CalcValue[] lstRet = new CalcValue[lstLeft.Count * count];

        for (int i = 0; i < count; i++) {
          for (int j = 0; j < lstLeft.Count; j++) {
            lstRet[i * lstLeft.Count + j] = lstLeft[j];
          }
        }

        return new CalcList(lstRet);
      }
      else {
        // Otherwise we'll multiply everything by the number.
        CalcValue[] lstRet = new CalcValue[lstLeft.Count];

        for (int i = 0; i < lstRet.Length; i++) {
          lstRet[i] = BinaryTimes.Run(lstLeft[i], numRight, vars, context);
        }
        return new CalcList(lstRet);
      }
    }
    #endregion
  }
}