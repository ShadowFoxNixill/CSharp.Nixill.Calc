using System.Linq;
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
    public static CLBinaryOperator BinaryPower { get; private set; }

    public static CLPrefixOperator PrefixMinus { get; private set; }

    public static void Load() {
      // First we need some local types
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
      BinaryTimes.AddFunction(num, lst, (left, right, vars, context) => BinTimesList(right, left, vars, context));
      BinaryTimes.AddFunction(num, str, (left, right, vars, context) => BinTimesString(right, left, vars, context));
      BinaryTimes.AddFunction(lst, lst, (left, right, vars, context) => BinTimesList(left, ListToNum(right), vars, context));
      BinaryTimes.AddFunction(lst, num, BinTimesList);
      BinaryTimes.AddFunction(str, num, BinTimesString);

      // The binary / operator
      BinaryDivide = CLOperators.BinaryOperators.GetOrNull("/") ?? new CLBinaryOperator("/", TimesPriority, true, true);
      BinaryDivide.AddFunction(num, num, BinDivideNumbers);
      BinaryDivide.AddFunction(num, lst, (left, right, vars, context) => BinDivideList(right, left, vars, context));
      BinaryDivide.AddFunction(lst, num, BinDivideList);
      BinaryDivide.AddFunction(lst, lst, (left, right, vars, context) => BinDivideList(left, ListToNum(right), vars, context));
      BinaryDivide.AddFunction(str, num, BinDivideStringException);
      BinaryDivide.AddFunction(num, str, BinDivideStringException);

      // The binary // operator
      BinaryIntDivide = CLOperators.BinaryOperators.GetOrNull("//") ?? new CLBinaryOperator("//", TimesPriority, true, true);
      BinaryIntDivide.AddFunction(num, num, BinIntDivideNumbers);
      BinaryIntDivide.AddFunction(lst, num, (left, right, vars, context) => BinIntDivideNumbers(ListToNum(left), right, vars, context));
      BinaryIntDivide.AddFunction(num, lst, (left, right, vars, context) => BinIntDivideNumbers(left, ListToNum(right), vars, context));
      BinaryIntDivide.AddFunction(lst, lst, (left, right, vars, context) => BinIntDivideNumbers(ListToNum(left), ListToNum(right), vars, context));

      // The binary % operator
      BinaryModulo = CLOperators.BinaryOperators.GetOrNull("%") ?? new CLBinaryOperator("%", TimesPriority, true, true);
      BinaryModulo.AddFunction(num, num, BinModuloNumbers);
      BinaryModulo.AddFunction(lst, num, (left, right, vars, context) => BinModuloNumbers(ListToNum(left), right, vars, context));
      BinaryModulo.AddFunction(num, lst, (left, right, vars, context) => BinModuloNumbers(left, ListToNum(right), vars, context));
      BinaryModulo.AddFunction(lst, lst, (left, right, vars, context) => BinModuloNumbers(ListToNum(left), ListToNum(right), vars, context));

      // The binary ^ operator
      BinaryPower = CLOperators.BinaryOperators.GetOrNull("^") ?? new CLBinaryOperator("^", TimesPriority, true, true);
      BinaryPower.AddFunction(num, num, BinPowerNumbers);
      BinaryPower.AddFunction(lst, num, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), right, vars, context));
      BinaryPower.AddFunction(num, lst, (left, right, vars, context) => BinPowerNumbers(left, ListToNum(right), vars, context));
      BinaryPower.AddFunction(lst, lst, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), ListToNum(right), vars, context));

      PrefixMinus = CLOperators.PrefixOperators.GetOrNull("-") ?? new CLPrefixOperator("-", PlusPriority, true);
      PrefixMinus.AddFunction(num, PreMinusNumber);
      PrefixMinus.AddFunction(lst, PreMinusList);
      PrefixMinus.AddFunction(str, PreMinusString);
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
      return BinaryPlus.Run(left, PrefixMinus.Run(right, vars, context), vars, context);
    }
    #endregion

    #region // BIN * FUNCTIONS //
    // Multiples two numbers and returns their product.
    private static CalcValue BinTimesNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft * numRight);
    }

    // Multiplies a list by a number.
    private static CalcValue BinTimesList(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
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

    // Multiplies a string by a number.
    private static CalcValue BinTimesString(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcString strLeft = left as CalcString;
      CalcNumber numRight = right as CalcNumber;

      int count = (int)numRight;
      if (count < 0) throw new CLException("Strings cannot be repeated negative times.");

      string strRet = "";

      for (int i = 0; i < count; i++) {
        strRet += strLeft;
      }

      return new CalcString(strRet);
    }
    #endregion

    #region // BIN / FUNCTIONS //
    // Returns the quotient of two numbers.
    private static CalcValue BinDivideNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft / numRight);
    }

    // Returns the quotient of a list's items over a number.
    private static CalcValue BinDivideList(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcList lstLeft = left as CalcList;
      CalcNumber numRight = right as CalcNumber;

      // We will *not* do string checking here
      // It's entirely possible someone writes a string divider and this function will use it.

      CalcValue[] lstRet = new CalcValue[lstLeft.Count];

      for (int i = 0; i < lstRet.Length; i++) {
        lstRet[i] = BinaryDivide.Run(lstLeft[i], numRight, vars, context);
      }

      return new CalcList(lstRet);
    }

    // Throws an exception, but exists. Can be overloaded by other modules.
    private static CalcValue BinDivideStringException(CalcObject left, CalcObject right, CLLocalStore vars, object context) =>
      throw new CLException("Strings cannot be divided by numbers.");
    #endregion

    #region // BIN // % ^ FUNCTIONS //
    // Returns the quotient, without remainder, of two numbers.
    private static CalcValue BinIntDivideNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(decimal.Floor(numLeft / numRight));
    }

    // Returns the quotient, without remainder, of two numbers.
    private static CalcValue BinModuloNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft % numRight);
    }

    // Returns the left raised to the power of the right.
    private static CalcValue BinPowerNumbers(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber((decimal)Math.Pow((double)(numLeft.Value), (double)(numRight.Value)));
    }
    #endregion

    #region // PREFIX - FUNCTIONS //
    // Returns the number, negated.
    private static CalcValue PreMinusNumber(CalcObject param, CLLocalStore vars, object context) {
      CalcNumber numParam = param as CalcNumber;

      return new CalcNumber(-numParam);
    }

    // Returns the string, reversed.
    private static CalcValue PreMinusString(CalcObject param, CLLocalStore vars, object context) {
      CalcString strParam = param as CalcString;

      char[] chars = strParam.Value.ToCharArray();
      Array.Reverse(chars);

      return new CalcString(new string(chars));
    }

    // Returns the list, with all its elements negated.
    private static CalcValue PreMinusList(CalcObject param, CLLocalStore vars, object context) {
      CalcList lstParam = param as CalcList;

      CalcValue[] lstRet = new CalcValue[lstParam.Count];

      for (int i = 0; i < lstRet.Length; i++) {
        lstRet[i] = PrefixMinus.Run(lstParam[i], vars, context);
      }

      return new CalcList(lstRet);
    }
    #endregion
  }
}