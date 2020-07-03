using System;
using System.Collections.Generic;
using Nixill.CalcLib.Functions;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcExpression : CalcObject { }

  public class CalcCodeFunction : CalcExpression {
    public CalcCodeFunctionDef Function { get; private set; }
    public List<CalcObject> Params;

    public sealed override bool Equals(object obj) {
      if (!(obj is CalcCodeFunction ccf)) return false;

      return ToCode() == ccf.ToCode();
    }

    public override int GetHashCode() {
      return ToCode().GetHashCode();
    }

    public override CalcValue GetValue() =>
      Function.FunctionDef.Invoke(Params);

    public override string ToCode() {
      string ret = "{!" + Function.Name;

      foreach (CalcObject obj in Params) {
        ret += "," + obj.ToCode();
      }

      return ret + "}";
    }

    public override string ToString(int level) {
      string ret = "{!" + Function.Name;

      if (Params.Count >= 1) {
        if (level == 0) ret += ", ...";
        else foreach (CalcObject obj in Params) {
            ret += ", " + obj.ToCode();
          }
      }

      return ret + "}";
    }
  }
}