using System;
using System.Collections;
using System.Collections.Generic;
using Nixill.CalcLib.Functions;
using Nixill.CalcLib.Varaibles;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Operators;

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

    public override CalcValue GetValue(CLLocalStore vars, object context = null) =>
      Function.FunctionDef.Invoke(Params, context, vars);

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

    public override CalcValue GetValue(CLLocalStore vars, object context = null) {
      CalcValue[] ret = new CalcValue[_list.Length];

      for (int i = 0; i < _list.Length; i++) {
        ret[i] = _list[i].GetValue(vars);
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

    public CalcFunction(string name, CalcObject[] pars) {
      Name = name;
      Params = pars;
    }

    public override bool Equals(object other) {
      if (!(other is CalcFunction func)) return false;

      return ToCode() == func.ToCode();
    }

    public override int GetHashCode() => ToCode().GetHashCode();

    public CalcObject GetObject(CLLocalStore vars = null, object context = null) {
      if (Name.StartsWith("*") || Name.StartsWith("^")) {
        if (!(vars.ContainsVar(Name))) throw new CalcException("No variable named " + Name + " exists.");
        else return vars[Name];
      }

      int count = 0;
      if (Int32.TryParse(Name, out count)) {
        if (vars.VarCount > count) return vars[count];
        else if (Params.Length > 0) return Params[1];
        else throw new CalcException("No parameter #" + count + " exists.");
      }

      if (Name == "...") {
        return new CalcListExpression(vars.CopyParams());
      }

      CalcObject ret = CLVariables.Load(Name, context);
      if (ret != null) return ret;

      throw new CalcException("No variable named " + Name + " exists.");
    }

    public override CalcValue GetValue(CLLocalStore vars, object context = null) {
      CalcObject obj = GetObject(vars, context);
      return obj.GetValue(vars);
    }

    public override string ToCode() {
      string ret = "{" + Name;

      foreach (CalcObject obj in Params) {
        ret += "," + obj.ToCode();
      }

      return ret + "}";
    }

    public override string ToString(int level) {
      string ret = "{" + Name;

      foreach (CalcObject obj in Params) {
        ret += ", " + obj.ToString(level - 1);
      }

      return ret + "}";
    }
  }

  public class CalcOperation : CalcExpression {
    public CalcObject Left { get; private set; }
    public CalcObject Right { get; private set; }
    public CLOperator Operator { get; private set; }

    public override bool Equals(object other) {
      if (!(other is CalcOperation oper)) return false;

      return ToCode() == oper.ToCode();
    }

    public override int GetHashCode() => ToCode().GetHashCode();

    public override string ToCode() {
      throw new NotImplementedException();
    }

    public override string ToString(int level) {
      throw new NotImplementedException();
    }

    public override CalcValue GetValue(CLLocalStore store, object context = null) {
      throw new NotImplementedException();
    }
  }
}