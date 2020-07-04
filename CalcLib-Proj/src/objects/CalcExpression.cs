using System;
using System.Collections;
using System.Collections.Generic;
using Nixill.CalcLib.Functions;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcExpression : CalcObject { }

  public class CalcCodeFunction : CalcExpression {
    public CLCodeFunction Function { get; private set; }
    CalcObject[] Params;
    public CalcObject this[int index] => Params[index];
    public int Count => Params.Length;

    public sealed override bool Equals(object obj) {
      if (!(obj is CalcCodeFunction ccf)) return false;

      return ToCode() == ccf.ToCode();
    }

    public override int GetHashCode() => ToCode().GetHashCode();

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

      if (Params.Length >= 1) {
        if (level == 0) ret += ", ...";
        else foreach (CalcObject obj in Params) {
            ret += ", " + obj.ToString(level - 1);
          }
      }

      return ret + "}";
    }
  }

  public class CalcListExpression : CalcExpression, IEnumerable<CalcObject> {
    private CalcObject[] _list;
    public CalcObject this[int index] => _list[index];
    public int Count => _list.Length;

    public IEnumerator<CalcObject> GetEnumerator() => ((IEnumerable<CalcObject>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public CalcListExpression(CalcObject[] list) {
      _list = new CalcObject[list.Length];
      for (int i = 0; i < list.Length; i++) {
        _list[i] = list[i];
      }
    }

    public override CalcValue GetValue() {
      CalcValue[] ret = new CalcValue[_list.Length];

      for (int i = 0; i < _list.Length; i++) {
        ret[i] = _list[i].GetValue();
      }

      return new CalcList(ret);
    }

    public override string ToString(int level) {
      string ret = "[";

      if (level == 0) ret += " ... ";
      else {
        foreach (CalcObject cv in _list) {
          ret += cv.ToString(level - 1) + ", ";
        }
        ret = ret.Substring(0, ret.Length - 2);
      }

      return ret + "]";
    }

    public override string ToCode() {
      string ret = "[";

      foreach (CalcObject cv in _list) {
        ret += cv.ToCode() + ",";
      }

      return ret.Substring(0, ret.Length - 1) + "]";
    }

    public override bool Equals(object other) {
      if (!(other is CalcList list)) return false;
      if (Count != list.Count) return false;

      for (int i = 0; i < Count; i++) {
        if (!(_list[i].Equals(list[i]))) return false;
      }

      return true;
    }

    public override int GetHashCode() {
      int hash = 0;
      foreach (CalcObject val in _list) {
        hash ^= val.GetHashCode();
      }
      return hash;
    }
  }

  public class CalcFunction : CalcExpression {
    public string Name { get; private set; }
    CalcObject[] Params;

    public override bool Equals(object other) {
      if (!(other is CalcFunction func)) return false;

      return ToCode() == func.ToCode();
    }

    public override int GetHashCode() => ToCode().GetHashCode();

    public CalcObject GetObject() {

    }

    public override CalcValue GetValue() {
      throw new NotImplementedException();
    }

    public override string ToCode() {
      throw new NotImplementedException();
    }

    public override string ToString(int level) {
      throw new NotImplementedException();
    }
  }
}