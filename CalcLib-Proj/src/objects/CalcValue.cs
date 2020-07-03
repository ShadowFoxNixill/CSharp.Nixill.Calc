using System.Collections.Generic;
using System.Collections;
using Nixill.CalcLib.Exception;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcValue : CalcObject {
    public override CalcValue GetValue() => this;
  }

  public class CalcInteger : CalcValue {
    public long Value { get; }

    public CalcInteger(long value) {
      Value = value;
    }

    public static implicit operator long(CalcInteger i) => i.Value;
    public static implicit operator CalcInteger(long l) => new CalcInteger(l);
    public static explicit operator CalcInteger(CalcDecimal d) => new CalcInteger((long)(d.Value));

    public override string ToString(int level) => Value.ToString();
    public override string ToCode() {
      if (Value >= 0)
        return Value.ToString();
      else
        return "(" + Value.ToString() + ")";
    }
  }

  public class CalcDecimal : CalcValue {
    private const string DISPLAY_FORMAT = "0.###;-0.###";
    private const string CODE_FORMAT = "0.###############;(-0.###############)";

    public double Value { get; }

    public CalcDecimal(double value) {
      Value = value;
    }

    public static implicit operator double(CalcDecimal d) => d.Value;
    public static implicit operator CalcDecimal(double d) => new CalcDecimal(d);
    public static explicit operator CalcDecimal(CalcInteger i) => new CalcDecimal((double)(i.Value));

    public override string ToString(int level) => Value.ToString(DISPLAY_FORMAT);
    public override string ToCode() => Value.ToString(CODE_FORMAT);
  }

  public class CalcList : CalcValue, IEnumerable<CalcValue> {
    private CalcValue[] _list;
    public CalcValue this[int index] => _list[index];
    public int Count => _list.Length;

    public IEnumerator<CalcValue> GetEnumerator() => ((IEnumerable<CalcValue>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public CalcList(CalcValue[] list) {
      _list = new CalcValue[list.Length];
      for (int i = 0; i < list.Length; i++) {
        _list[i] = list[i];
      }
    }

    public override string ToString(int level) {
      string ret;

      if (!HasString()) ret = "[";
      else ret = Sum() + " [";

      if (level == 0) ret += " ... ";
      else {
        foreach (CalcValue cv in _list) {
          ret += cv.ToString(level - 1) + ", ";
        }
        ret = ret.Substring(0, ret.Length - 2);
      }

      return ret + "]";
    }

    public override string ToCode() {
      string ret = "[";

      foreach (CalcValue cv in _list) {
        ret += cv.ToCode() + ",";
      }

      return ret.Substring(0, ret.Length - 1) + "]";
    }

    public bool HasString() {
      foreach (CalcValue val in _list) {
        if (val is CalcString) return true;
        else if (val is CalcList lst) {
          if (lst.HasString()) return true;
        }
      }
      return false;
    }

    public double Sum() {
      double sum = 0;
      foreach (CalcValue val in _list) {
        if (val is CalcString) throw new CalcException("Strings cannot be summed.");
        if (val is CalcList cl) sum += cl.Sum();
        else if (val is CalcDecimal cd) sum += cd.Value;
        else if (val is CalcInteger ci) sum += (double)ci.Value;
      }
      return sum;
    }
  }

  public class CalcString : CalcValue {
    public string Value { get; }

    public CalcString(string value) {
      Value = value;
    }

    public static implicit operator string(CalcString s) => s.Value;
    public static implicit operator CalcString(string s) => new CalcString(s);

    public override string ToString(int level) {
      return Value;
    }

    public override string ToCode() {
      return "\"" + Value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
  }
}