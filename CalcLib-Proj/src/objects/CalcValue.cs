using System;
using System.Collections.Generic;
using System.Collections;
using Nixill.CalcLib.Exception;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcObject : CalcObject {
    public override CalcObject GetValue() => this;
  }

  public class CalcInteger : CalcObject, IComparable<CalcInteger>, IComparable<long>, IComparable<CalcDecimal>, IComparable<double> {
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

    public override bool Equals(object other) {
      if (other is CalcInteger i) return Value == i.Value;
      if (other is CalcDecimal d) return Value == d.Value;
      return false;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(CalcInteger other) => Value.CompareTo(other.Value);
    public int CompareTo(long other) => Value.CompareTo(other);
    public int CompareTo(CalcDecimal other) => Value.CompareTo(other.Value);
    public int CompareTo(double other) => Value.CompareTo(other);
  }

  public class CalcDecimal : CalcObject, IComparable<CalcDecimal>, IComparable<double>, IComparable<CalcInteger>, IComparable<long> {
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

    public override bool Equals(object other) {
      if (other is CalcInteger i) return Value == i.Value;
      if (other is CalcDecimal d) return Value == d.Value;
      return false;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(CalcDecimal other) => Value.CompareTo(other);
    public int CompareTo(double other) => Value.CompareTo(other);
    public int CompareTo(CalcInteger other) => Value.CompareTo(other);
    public int CompareTo(long other) => Value.CompareTo(other);
  }

  public class CalcList : CalcObject, IEnumerable<CalcObject> {
    private CalcObject[] _list;
    public CalcObject this[int index] => _list[index];
    public int Count => _list.Length;

    public IEnumerator<CalcObject> GetEnumerator() => ((IEnumerable<CalcObject>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public CalcList(CalcObject[] list) {
      _list = new CalcObject[list.Length];
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

    public bool HasString() {
      foreach (CalcObject val in _list) {
        if (val is CalcString) return true;
        else if (val is CalcList lst) {
          if (lst.HasString()) return true;
        }
      }
      return false;
    }

    public double Sum() {
      double sum = 0;
      foreach (CalcObject val in _list) {
        if (val is CalcString) throw new CalcException("Strings cannot be summed.");
        if (val is CalcList cl) sum += cl.Sum();
        else if (val is CalcDecimal cd) sum += cd.Value;
        else if (val is CalcInteger ci) sum += (double)ci.Value;
      }
      return sum;
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

  public class CalcString : CalcObject, IComparable<CalcString>, IComparable<String> {
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

    public override bool Equals(object other) {
      if (!(other is CalcString str)) return false;
      return Value == str.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(string other) => Value.CompareTo(other);
    public int CompareTo(CalcString other) => Value.CompareTo(other);
  }
}