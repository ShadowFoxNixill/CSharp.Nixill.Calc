using System.Linq;
using System;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;
using Nixill.CalcLib.Varaibles;
using static Nixill.CalcLib.Modules.Casting;

namespace Nixill.CalcLib.Modules
{
  public class MainModule
  {
    public const int PlusPriority = 0;
    public const int TimesPriority = PlusPriority + 5;
    public const int PowerPriority = TimesPriority + 10;

    public static CLBinaryOperator BinaryPlus { get; private set; }
    public static CLBinaryOperator BinaryMinus { get; private set; }
    public static CLBinaryOperator BinaryTimes { get; private set; }
    public static CLBinaryOperator BinaryDivide { get; private set; }
    public static CLBinaryOperator BinaryIntDivide { get; private set; }
    public static CLBinaryOperator BinaryModulo { get; private set; }
    public static CLBinaryOperator BinaryPower { get; private set; }

    public static CLPrefixOperator PrefixMinus { get; private set; }

    public static void Load(int plusPriority = PlusPriority, int timesPriority = TimesPriority, int powerPriority = PowerPriority)
    {
      LoadBinaryPlus(plusPriority);
      LoadBinaryMinus(plusPriority);
      LoadBinaryTimes(timesPriority);
      LoadBinaryDivide(timesPriority);
      LoadBinaryIntDivide(timesPriority);
      LoadBinaryModulo(timesPriority);
      LoadBinaryPower(powerPriority);
      LoadPrefixMinus(plusPriority);
    }

    #region // BIN + FUNCTIONS //
    public static CLBinaryOperator LoadBinaryPlus(int priority = 0, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryPlus = CLOperators.BinaryOperators.GetOrNull("+") ?? new CLBinaryOperator("+", priority, valOnLeft, valOnRight);
      BinaryPlus.AddFunction(tNum, tNum, BinPlusNumbers);
      BinaryPlus.AddFunction(tNum, tStr, (left, right, vars, context) => BinPlusStrings(NumToString(left), right, vars, context));
      BinaryPlus.AddFunction(tStr, tNum, (left, right, vars, context) => BinPlusStrings(left, NumToString(right), vars, context));
      BinaryPlus.AddFunction(tStr, tStr, BinPlusStrings);
      BinaryPlus.AddFunction(tVal, tLst, (left, right, vars, context) => BinPlusLists(ValToList(left), right, vars, context));
      BinaryPlus.AddFunction(tLst, tVal, (left, right, vars, context) => BinPlusLists(left, ValToList(right), vars, context));
      BinaryPlus.AddFunction(tLst, tLst, BinPlusLists);

      return BinaryPlus;
    }

    // Adds two numbers.
    public static CalcValue BinPlusNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft + numRight);
    }

    // Concatenates two strings.
    public static CalcValue BinPlusStrings(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcString strLeft = left as CalcString;
      CalcString strRight = right as CalcString;

      return new CalcString(strLeft + strRight);
    }

    // Concatenates two lists.
    public static CalcValue BinPlusLists(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcList lstLeft = left as CalcList;
      CalcList lstRight = right as CalcList;

      CalcValue[] lstRet = new CalcValue[lstLeft.Count + lstRight.Count];

      int i;

      for (i = 0; i < lstLeft.Count; i++)
      {
        lstRet[i] = lstLeft[i];
      }

      for (int j = 0; j < lstRight.Count; j++)
      {
        lstRet[i + j] = lstRight[j];
      }

      return new CalcList(lstRet);
    }
    #endregion

    #region // BIN - FUNCTION //
    public static CLBinaryOperator LoadBinaryMinus(int priority = 0, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryMinus = CLOperators.BinaryOperators.GetOrNull("-") ?? new CLBinaryOperator("-", priority, valOnLeft, valOnRight);
      BinaryMinus.AddFunction(tNum, tNum, BinMinusNumbers);
      BinaryMinus.AddFunction(tVal, tLst, (left, right, vars, context) => BinMinusLists(ValToList(left), right, vars, context));
      BinaryMinus.AddFunction(tLst, tVal, (left, right, vars, context) => BinMinusLists(left, ValToList(right), vars, context));
      BinaryMinus.AddFunction(tLst, tLst, BinMinusLists);

      return BinaryMinus;
    }

    // Adds two numbers.
    public static CalcValue BinMinusNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft - numRight);
    }

    // Concatenates two lists.
    public static CalcValue BinMinusLists(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcList lstLeft = left as CalcList;
      CalcList lstRight = PreMinusList(right, vars, context) as CalcList;

      CalcValue[] lstRet = new CalcValue[lstLeft.Count + lstRight.Count];

      int i;

      for (i = 0; i < lstLeft.Count; i++)
      {
        lstRet[i] = lstLeft[i];
      }

      for (int j = 0; j < lstRight.Count; j++)
      {
        lstRet[i + j] = lstRight[j];
      }

      return new CalcList(lstRet);
    }
    #endregion

    #region // BIN * FUNCTIONS //
    public static CLBinaryOperator LoadBinaryTimes(int priority = TimesPriority, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryTimes = CLOperators.BinaryOperators.GetOrNull("*") ?? new CLBinaryOperator("*", priority, valOnLeft, valOnRight);
      BinaryTimes.AddFunction(tNum, tNum, BinTimesNumbers);
      BinaryTimes.AddFunction(tNum, tLst, (left, right, vars, context) => BinTimesList(right, left, vars, context));
      BinaryTimes.AddFunction(tNum, tStr, (left, right, vars, context) => BinTimesString(right, left, vars, context));
      BinaryTimes.AddFunction(tLst, tLst, (left, right, vars, context) => BinTimesList(left, ListToNum(right), vars, context));
      BinaryTimes.AddFunction(tLst, tNum, BinTimesList);
      BinaryTimes.AddFunction(tStr, tNum, BinTimesString);

      return BinaryTimes;
    }

    // Multiples two numbers and returns their product.
    public static CalcValue BinTimesNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft * numRight);
    }

    // Multiplies a list by a number.
    public static CalcValue BinTimesList(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcList lstLeft = left as CalcList;
      CalcNumber numRight = right as CalcNumber;

      // If it has a string, we'll just repeat the list multiple times.
      if (lstLeft.HasString())
      {
        int count = (int)numRight;
        if (count < 0) throw new CLException("Lists cannot be repeated negative times.");

        CalcValue[] lstRet = new CalcValue[lstLeft.Count * count];

        for (int i = 0; i < count; i++)
        {
          for (int j = 0; j < lstLeft.Count; j++)
          {
            lstRet[i * lstLeft.Count + j] = lstLeft[j];
          }
        }

        return new CalcList(lstRet);
      }
      else
      {
        // Otherwise we'll multiply everything by the number.
        CalcValue[] lstRet = new CalcValue[lstLeft.Count];

        for (int i = 0; i < lstRet.Length; i++)
        {
          lstRet[i] = BinaryTimes.Run(lstLeft[i], numRight, vars, context);
        }
        return new CalcList(lstRet);
      }
    }

    // Multiplies a string by a number.
    public static CalcValue BinTimesString(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcString strLeft = left as CalcString;
      CalcNumber numRight = right as CalcNumber;

      int count = (int)numRight;
      if (count < 0) throw new CLException("Strings cannot be repeated negative times.");

      string strRet = "";

      for (int i = 0; i < count; i++)
      {
        strRet += strLeft;
      }

      return new CalcString(strRet);
    }
    #endregion

    #region // BIN / FUNCTIONS //
    public static CLBinaryOperator LoadBinaryDivide(int priority = TimesPriority, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryDivide = CLOperators.BinaryOperators.GetOrNull("/") ?? new CLBinaryOperator("/", priority, valOnLeft, valOnRight);
      BinaryDivide.AddFunction(tNum, tNum, BinDivideNumbers);
      BinaryDivide.AddFunction(tNum, tLst, (left, right, vars, context) => BinDivideNumbers(left, ListToNum(right), vars, context));
      BinaryDivide.AddFunction(tLst, tNum, BinDivideList);
      BinaryDivide.AddFunction(tLst, tLst, (left, right, vars, context) => BinDivideList(left, ListToNum(right), vars, context));
      BinaryDivide.AddFunction(tStr, tNum, BinDivideStringException);
      BinaryDivide.AddFunction(tNum, tStr, BinDivideStringException);

      return BinaryDivide;
    }

    // Returns the quotient of two numbers.
    public static CalcValue BinDivideNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft / numRight);
    }

    // Returns the quotient of a list's items over a number.
    public static CalcValue BinDivideList(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcList lstLeft = left as CalcList;
      CalcNumber numRight = right as CalcNumber;

      // We will *not* do string checking here
      // It's entirely possible someone writes a string divider and this function will use it.

      CalcValue[] lstRet = new CalcValue[lstLeft.Count];

      for (int i = 0; i < lstRet.Length; i++)
      {
        lstRet[i] = BinaryDivide.Run(lstLeft[i], numRight, vars, context);
      }

      return new CalcList(lstRet);
    }

    // Throws an exception, but exists. Can be overloaded by other modules.
    public static CalcValue BinDivideStringException(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context) =>
      throw new CLException("Strings cannot be divided by numbers.");
    #endregion

    #region // BIN // % ^ FUNCTIONS //
    // Returns the quotient, without remainder, of two numbers.
    public static CLBinaryOperator LoadBinaryIntDivide(int priority = TimesPriority, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryIntDivide = CLOperators.BinaryOperators.GetOrNull("//") ?? new CLBinaryOperator("//", priority, valOnLeft, valOnRight);
      BinaryIntDivide.AddFunction(tNum, tNum, BinIntDivideNumbers);
      BinaryIntDivide.AddFunction(tLst, tNum, (left, right, vars, context) => BinIntDivideNumbers(ListToNum(left), right, vars, context));
      BinaryIntDivide.AddFunction(tNum, tLst, (left, right, vars, context) => BinIntDivideNumbers(left, ListToNum(right), vars, context));
      BinaryIntDivide.AddFunction(tLst, tLst, (left, right, vars, context) => BinIntDivideNumbers(ListToNum(left), ListToNum(right), vars, context));

      return BinaryIntDivide;
    }

    public static CalcValue BinIntDivideNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(decimal.Floor(numLeft / numRight));
    }

    public static CLBinaryOperator LoadBinaryModulo(int priority = TimesPriority, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryModulo = CLOperators.BinaryOperators.GetOrNull("%") ?? new CLBinaryOperator("%", priority, valOnLeft, valOnRight);
      BinaryModulo.AddFunction(tNum, tNum, BinModuloNumbers);
      BinaryModulo.AddFunction(tLst, tNum, (left, right, vars, context) => BinModuloNumbers(ListToNum(left), right, vars, context));
      BinaryModulo.AddFunction(tNum, tLst, (left, right, vars, context) => BinModuloNumbers(left, ListToNum(right), vars, context));
      BinaryModulo.AddFunction(tLst, tLst, (left, right, vars, context) => BinModuloNumbers(ListToNum(left), ListToNum(right), vars, context));

      return BinaryModulo;
    }

    // Returns the quotient, without remainder, of two numbers.
    public static CalcValue BinModuloNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber(numLeft % numRight);
    }

    public static CLBinaryOperator LoadBinaryPower(int priority = PowerPriority, bool valOnLeft = true, bool valOnRight = true)
    {
      BinaryPower = CLOperators.BinaryOperators.GetOrNull("^");
      if (BinaryPower == null)
      {
        BinaryPower = new CLBinaryOperator("^", priority, valOnLeft, valOnRight);
        CLOperators.SetFromRight(PowerPriority);
      }
      BinaryPower.AddFunction(tNum, tNum, BinPowerNumbers);
      BinaryPower.AddFunction(tLst, tNum, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), right, vars, context));
      BinaryPower.AddFunction(tNum, tLst, (left, right, vars, context) => BinPowerNumbers(left, ListToNum(right), vars, context));
      BinaryPower.AddFunction(tLst, tLst, (left, right, vars, context) => BinPowerNumbers(ListToNum(left), ListToNum(right), vars, context));

      return BinaryPower;
    }

    // Returns the left raised to the power of the right.
    public static CalcValue BinPowerNumbers(CalcObject left, CalcObject right, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numLeft = left as CalcNumber;
      CalcNumber numRight = right as CalcNumber;

      return new CalcNumber((decimal)Math.Pow((double)(numLeft.Value), (double)(numRight.Value)));
    }
    #endregion

    #region // PREFIX - FUNCTIONS //
    public static CLPrefixOperator LoadPrefixMinus(int priority = PlusPriority, bool valueBased = true)
    {
      PrefixMinus = CLOperators.PrefixOperators.GetOrNull("-") ?? new CLPrefixOperator("-", PlusPriority, true);
      PrefixMinus.AddFunction(tNum, PreMinusNumber);
      PrefixMinus.AddFunction(tLst, PreMinusList);
      PrefixMinus.AddFunction(tStr, PreMinusString);

      return PrefixMinus;
    }

    // Returns the number, negated.
    public static CalcValue PreMinusNumber(CalcObject param, CLLocalStore vars, CLContextProvider context)
    {
      CalcNumber numParam = param as CalcNumber;

      return new CalcNumber(-numParam);
    }

    // Returns the string, reversed.
    public static CalcValue PreMinusString(CalcObject param, CLLocalStore vars, CLContextProvider context)
    {
      CalcString strParam = param as CalcString;

      char[] chars = strParam.Value.ToCharArray();
      Array.Reverse(chars);

      return new CalcString(new string(chars));
    }

    // Returns the list, with all its elements negated.
    public static CalcValue PreMinusList(CalcObject param, CLLocalStore vars, CLContextProvider context)
    {
      CalcList lstParam = param as CalcList;

      CalcValue[] lstRet = new CalcValue[lstParam.Count];

      for (int i = 0; i < lstRet.Length; i++)
      {
        lstRet[i] = PrefixMinus.Run(lstParam[i], vars, context);
      }

      return new CalcList(lstRet);
    }
    #endregion
  }
}