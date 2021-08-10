using System;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Modules {
  public static class Casting {
    public static Type tNum = typeof(CalcNumber);
    public static Type tLst = typeof(CalcList);
    public static Type tStr = typeof(CalcString);
    public static Type tVal = typeof(CalcValue);

    // Converts a number to a string.
    public static CalcString NumToString(CalcObject num) => new CalcString((num as CalcNumber).Value.ToString("0.#####"));

    // Converts a value to a list.
    public static CalcList ValToList(CalcObject val) => new CalcList(new CalcValue[] { (val as CalcValue) });

    // Converts a list to a number.
    public static CalcNumber ListToNum(CalcObject lst) => new CalcNumber((lst as CalcList).Sum());

    // Gets the parameter at a given index in the params list as a number.
    public static CalcNumber NumberAt(CalcObject[] pars, int index, string name, CLLocalStore vars, CLContextProvider context) {
      if (pars.Length <= index) throw new CLException(name + " parameter " + index + " was not specified.");
      CalcValue val = pars[index].GetValue(vars, context);
      if (!(val is CalcNumber num)) throw new CLCastException(name + " parameter " + index + " must be a number.");
      return num;
    }

    // Gets the parameter at a given index in the params list as a value.
    public static CalcValue ValueAt(CalcObject[] pars, int index, string name, CLLocalStore vars, CLContextProvider context) {
      if (pars.Length <= index) throw new CLException($"{name} parameter {index} was not specified.");
      CalcValue val = pars[index].GetValue(vars, context);
      return val;
    }
  }
}