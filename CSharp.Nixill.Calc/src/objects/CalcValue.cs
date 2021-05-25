using System;
using System.Collections.Generic;
using System.Collections;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Objects {
  /// <summary>
  /// Represents a resolved value.
  /// </summary>
  public abstract class CalcValue : CalcObject {
    public sealed override CalcValue GetValue(CLLocalStore vars, CLContextProvider context = null) => this;
  }

  /// <summary>
  /// Represents a number.
  /// </summary>
  public class CalcNumber : CalcValue, IComparable<CalcNumber>, IComparable<decimal> {
    private const string DISPLAY_FORMAT = "0.###;-0.###";
    private const string CODE_FORMAT = "0.###############;(-0.###############)";

    /// <summary>The value of the contained number.</summary>
    public decimal Value { get; }

    /// <summary>Creates a new <c>CalcNumber</c>.</summary>
    public CalcNumber(decimal value) {
      Value = Decimal.Round(value, 15);
    }

    public static implicit operator decimal(CalcNumber d) => d.Value;
    public static implicit operator CalcNumber(decimal d) => new CalcNumber(d);
    public static explicit operator CalcNumber(double d) => new CalcNumber((decimal)d);
    public static explicit operator CalcNumber(float d) => new CalcNumber((decimal)d);

    public override string ToString(int level) => Value.ToString(DISPLAY_FORMAT);
    public override string ToCode() => Value.ToString(CODE_FORMAT);

    public override bool Equals(object other) {
      if (other is CalcNumber d) return Value == d.Value;
      return false;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(CalcNumber other) => Value.CompareTo(other.Value);
    public int CompareTo(decimal other) => Value.CompareTo(Decimal.Round(other, 15));

    public override string ToTree(int level) {
      return new String(' ', level * 2) + "Number: " + Value.ToString(CODE_FORMAT);
    }
  }

  /// <summary>
  /// Represents a list of values.
  /// </summary>
  /// <remarks>
  /// <para>This class is differentiated from <c>CalcListExpression</c> by
  ///   the fact that this can only hold resolved values.</para>
  /// </remarks>
  /// <seealso cref="CalcListExpression"/>
  public class CalcList : CalcValue, IEnumerable<CalcValue> {
    private CalcValue[] _list;

    /// <summary>Retrieves a specific item from the <c>CalcList</c>.</summary>
    public CalcValue this[int index] => _list[index];

    /// <summary>Returns the size of the <c>CalcList</c>.</summary>
    public int Count => _list.Length;

    /// <summary>
    /// Returns an enumerator through the items of the list.
    /// </summary>
    public IEnumerator<CalcValue> GetEnumerator() => ((IEnumerable<CalcValue>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    /// <summary>
    /// Creates a new <c>CalcList</c>.
    /// </summary>
    /// <param name="list">The values going into the list.</param>
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

    /// <summary>
    /// Returns whether there are any <c>CalcString</c>s in this
    ///   <c>CalcList</c> or any child <c>CalcList</c>s.
    /// </summary>
    public bool HasString() {
      foreach (CalcValue val in _list) {
        if (val is CalcString) return true;
        else if (val is CalcList lst) {
          if (lst.HasString()) return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the sum of all the entries in the <c>CalcList</c>.
    /// <summary>
    /// <remarks>
    /// <para>If there are any <c>CalcString</c>s within this or child
    ///   <c>CalcList</c>s, this operation will throw a
    ///   <c>CalcException</c>.</para>
    /// </remarks>
    public decimal Sum() {
      decimal sum = 0;
      foreach (CalcValue val in _list) {
        if (val is CalcString) throw new CLException("Strings cannot be summed.");
        if (val is CalcList cl) sum += cl.Sum();
        else if (val is CalcNumber cd) sum += cd.Value;
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
      foreach (CalcValue val in _list) {
        hash ^= val.GetHashCode();
      }
      return hash;
    }

    public override string ToTree(int level) {
      string ret = new string(' ', level * 2) + "List:";
      if (_list.Length == 0) return ret + " (empty)";

      foreach (CalcValue val in _list) {
        ret += "\n" + val.ToTree(level + 1);
      }

      return ret;
    }
  }

  /// <summary>
  /// Represents a string of text.
  /// </summary>
  public class CalcString : CalcValue, IComparable<CalcString>, IComparable<String> {
    /// <summary>The contained string.</summary>
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

    public override string ToTree(int level) {
      return new String(' ', level * 2) + "String: " + ToCode();
    }

    public int CompareTo(string other) => Value.CompareTo(other);
    public int CompareTo(CalcString other) => Value.CompareTo(other);
  }
}