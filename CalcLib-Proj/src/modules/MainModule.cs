using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Operators;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Modules {
  public class MainModule {
    public static bool Loaded { get; private set; } = false;

    public const int PlusPriority = 0;

    public static CLBinaryOperator BinaryPlus { get; private set; }
    public static CLBinaryOperator BinaryMinus { get; private set; }
    public static CLBinaryOperator BinaryTimes { get; private set; }
    public static CLBinaryOperator BinaryDivide { get; private set; }
    public static CLBinaryOperator BinaryIntDivide { get; private set; }
    public static CLBinaryOperator BinaryModulo { get; private set; }

    public static CLBinaryOperator PrefixMinus { get; private set; }
    public static CLBinaryOperator PostfixBang { get; private set; }

    public static void Load() {
      BinaryPlus = new CLBinaryOperator("+", PlusPriority, BinPlusFunc);
      BinaryMinus = new CLBinaryOperator("-", PlusPriority, BinMinusFunc);
    }

    private static CalcValue BinPlusFunc(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      // First, this method only deals with values.
      CalcValue valLeft = left.GetValue(vars, context);
      CalcValue valRight = right.GetValue(vars, context);

      // If both values are numbers, we have a simple job.
      // Just output the sum of those numbers.
      CalcNumber numLeft = valLeft as CalcNumber;
      CalcNumber numRight = valRight as CalcNumber;

      if (numLeft != null && numRight != null) {
        return new CalcNumber(numLeft + numRight);
      }

      // If both values are strings, it's slightly more complicated.
      // We'll concatenate the strings.
      CalcString strLeft = valLeft as CalcString;
      CalcString strRight = valRight as CalcString;

      if ((numLeft != null || strLeft != null) && (numRight != null || strRight != null)) {
        // Numbers become strings with five decimal places.
        if (numLeft != null) strLeft = new CalcString(numLeft.Value.ToString("0.#####"));
        if (numRight != null) strRight = new CalcString(numRight.Value.ToString("0.#####"));

        return new CalcString(strLeft + strRight);
      }

      // Otherwise, let's concatenate them as lists.
      CalcList lstLeft = valLeft as CalcList;
      CalcList lstRight = valRight as CalcList;

      // Any value becomes a list by just becoming a list of only that one value.
      if (lstLeft == null) lstLeft = new CalcList(new CalcValue[] { valLeft });
      if (lstRight == null) lstRight = new CalcList(new CalcValue[] { valRight });

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

    private static CalcValue BinMinusFunc(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      return CLBinaryOperator.Get("+").Run(left, CLPrefixOperator.Get("-").Run(right, vars, context), vars, context);
    }

    private static CalcValue BinTimesFunc(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      // This method takes four forms.
      CalcValue valLeft = left.GetValue(vars, context);
      CalcValue valRight = right.GetValue(vars, context);

      // First form: Product of two numbers.
      CalcNumber numLeft = valLeft as CalcNumber;
      CalcNumber numRight = valRight as CalcNumber;

      if (numLeft != null && numRight != null) return new CalcNumber(numLeft * numRight);

      // Second and third forms: List times number.
      CalcList lstLeft = valLeft as CalcList;
      CalcList lstRight = valRight as CalcList;

      // Let's make this commutative more easily.
      if (numLeft != null && lstRight != null) {
        lstLeft = lstRight;
        numRight = numLeft;
        lstRight = null;
        numLeft = null;
      }
      // But if it's dual lists, the right list has to become a number instead.
      else if (lstLeft != null && lstRight != null) numRight = lstRight.Sum();

      if (lstLeft != null && numRight != null) {
        CalcValue[] lstRet;

        if (!lstLeft.HasString()) {
          // Returns the list with all numbers multiplied by this list entry.
          lstRet = new CalcValue[lstLeft.Count];
          for (int i = 0; i < lstLeft.Count; i++) {
            lstRet[i] = BinTimesFunc(lstLeft[i], right, vars, context);
          }
          return new CalcList(lstRet);
        }
        else {
          // Returns the list repeated the number times.
          int intRight = (int)(numRight);
          if (intRight < 0) throw new CLException("Lists containing strings cannot be repeated negative times.");

          lstRet = new CalcValue[lstLeft.Count * (intRight)];
          for (int j = 0; j < numRight; j++) {
            for (int i = 0; i < lstLeft.Count; i++) {
              lstRet[j * lstLeft.Count + i] = lstLeft[i];
            }
          }
          return new CalcList(lstRet);
        }
      }

      // Fourth form: String times number
      // (If the other is a list then it should be summed)
      CalcString strLeft = valLeft as CalcString;
      CalcString strRight = valRight as CalcString;

      // Swap if we have the string on the right
      if ((numLeft != null || lstLeft != null) && strRight != null) {
        strLeft = strRight;
        numRight = numLeft;
        lstRight = lstLeft;
        strRight = null;
        numLeft = null;
        lstLeft = null;
      }

      // If we have a list instead of a number, sum the list
      if (lstRight != null) numRight = lstRight.Sum();

      if (numRight == null) throw new CLException("Strings cannot be multiplied by strings.");

      string ret = "";
      int retCount = (int)(numRight);

      if (retCount < 0) throw new CLException("Strings cannot be repeated negative times.");

      for (int i = 0; i < retCount; i++) {
        ret += strLeft;
      }

      return new CalcString(ret);
    }

    private static CalcValue BinDivideFunc(CalcObject left, CalcObject right, CLLocalStore vars, object context) {
      // This method takes two forms.
      CalcValue valLeft = left.GetValue(vars, context);
      CalcValue valRight = right.GetValue(vars, context);

      // First off, strings are banned.
      if (valLeft is CalcString || valRight is CalcString) throw new CLException("Strings cannot be divided.");

      // First form: Product of two numbers.
      CalcNumber numLeft = valLeft as CalcNumber;
      CalcNumber numRight = valRight as CalcNumber;

      // If the right side's a list, we have to make it a number.
      CalcList lstRight = valRight as CalcList;
      if (lstRight != null) numRight = lstRight.Sum();

      if (numLeft != null && numRight != null) return new CalcNumber(numLeft / numRight);

      // Second form: List times number.
      CalcList lstLeft = valLeft as CalcList;

      if (lstLeft.HasString()) throw new CLException("Strings cannot be divided.");

      CalcValue[] lstRet = new CalcValue[lstLeft.Count];
      for (int i = 0; i < lstLeft.Count; i++) {
        lstRet[i] = BinTimesFunc(lstLeft[i], right, vars, context);
      }
      return new CalcList(lstRet);
    }
  }
}