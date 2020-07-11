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
      // This one's a bit less involved than the other.
    }
  }
}