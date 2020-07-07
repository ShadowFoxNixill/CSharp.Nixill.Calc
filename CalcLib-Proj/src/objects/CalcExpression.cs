using System;
using System.Collections;
using System.Collections.Generic;
using Nixill.CalcLib.Functions;
using Nixill.CalcLib.Varaibles;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Operators;

namespace Nixill.CalcLib.Objects {
  /// <summary>
  /// Base class for <c>CalcObject</c>s that are not <c>CalcValue</c>s.
  /// </summary>
  /// <seealso cref="CalcValue"/>
  public abstract class CalcExpression : CalcObject {
    /// <summary>
    /// Determines if two <c>CalcExpression</c>s are equal by comparing
    /// their code representations. (<c>CalcExpression</c>s are not equal
    /// to objects that are not <c>CalcExpression</c>s.)
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <seealso cref="ToCode()"/>
    public sealed override bool Equals(object obj) {
      if (!(obj is CalcExpression ccf)) return false;

      return ToCode() == ccf.ToCode();
    }

    /// <summary>
    /// Returns the hash code of the <c>CalcExpression</c>.
    /// </summary>
    public sealed override int GetHashCode() => ToCode().GetHashCode();

    /// <summary>
    /// Evaluates this <c>CalcExpression</c> and returns the result.
    /// </summary>
    /// <param name="vars">A <c>CLLocalStore</c> that stores local
    ///   variables.</param>
    /// <param name="context">The object representing the context in which
    ///   the expression is being evaluated.</param>
    public abstract override CalcValue GetValue(CLLocalStore vars, object context = null);
  }

  /// <summary>
  /// <c>CalcExpression</c>s that are hard-coded outside the scope of
  ///   CalcLib expressions.
  /// </summary>
  /// <inheritdoc/>
  public class CalcCodeFunction : CalcExpression {
    /// <summary>
    /// The <c>CLCodeFunction</c> backing this <c>CalcCodeFunction</c>.
    /// </summary>
    public CLCodeFunction Function { get; }

    CalcObject[] Params;

    /// <summary>
    /// Allows access to the parameters of a <c>CalcCodeFunction</c>.
    /// </summary>
    public CalcObject this[int index] => Params[index];

    /// <summary>
    /// How many parameters this <c>CalcCodeFunction</c> has.
    /// </summary>
    public int Count => Params.Length;

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

    public override string ToTree(int level) {
      string ret = new string(' ', level * 2) + "CodeFunction: " + Function.Name;

      if (Params.Length == 0) return ret + " (no params)";

      foreach (CalcObject obj in Params) {
        ret += "\n" + obj.ToTree(level + 1);
      }

      return ret;
    }
  }

  /// <summary>
  /// A list of <c>CalcObject</c>s.
  /// </summary>
  public class CalcListExpression : CalcExpression, IEnumerable<CalcObject> {
    private CalcObject[] _list;

    /// <summary>
    /// Gets the <c>CalcObject</c> at a certain position in the
    ///   <c>CalcList</c>.
    /// </summary>
    public CalcObject this[int index] => _list[index];

    /// <summary>The length of the <c>CalcList</c>.</summary>
    public int Count => _list.Length;

    public IEnumerator<CalcObject> GetEnumerator() => ((IEnumerable<CalcObject>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    /// <summary>Creates a new <c>CalcList</c>.</summary>
    /// <param name="list">The list to copy.</param>
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

    public override string ToTree(int level) {
      string ret = new string(' ', level * 2) + "ListExpression:";

      if (_list.Length == 0) return ret + " (empty)";

      foreach (CalcObject obj in _list) {
        ret += "\n" + obj.ToTree(level + 1);
      }

      return ret;
    }
  }

  /// <summary>
  /// A named function or variable referring to a stored
  ///   <c>CalcObject</c>.
  /// </summary>
  public class CalcFunction : CalcExpression {
    /// <summary>The name of the function.</summary>
    public string Name { get; private set; }
    private CalcObject[] Params;

    /// <summary>
    /// Gets the <c>CalcObject</c> at a certain position in the
    ///   <c>CalcFunction</c>.
    /// </summary>
    public CalcObject this[int index] => Params[index];

    /// <summary>The length of the <c>CalcList</c>.</summary>
    public int Count => Params.Length;

    /// <summary>
    /// Creates a new <c>CalcFunction</c>.
    /// </summary>
    /// <param name="name">The name of the function to call.</param>
    /// <param name="pars">The parameters for the called function.</param>
    public CalcFunction(string name, CalcObject[] pars) {
      Name = name;
      Params = pars;
    }

    /// <summary>
    /// Returns the object referenced by the name.
    /// </summary>0
    /// <param name="vars">A <c>CLLocalStore</c> that stores local
    ///   variables.</param>
    /// <param name="context">The object representing the context in which
    ///   the expression is being evaluated.</param>
    public CalcObject GetObject(CLLocalStore vars = null, object context = null) {
      vars = vars ?? new CLLocalStore();

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

    public override string ToTree(int level) {
      string ret = new string(' ', level * 2) + "CodeFunction: " + Name;

      if (Params.Length == 0) return ret + " (no params)";

      foreach (CalcObject obj in Params) {
        ret += "\n" + obj.ToTree(level + 1);
      }

      return ret;
    }
  }

  /// <summary>
  /// A simple operation on one or two <c>CalcValue</c>s.
  /// </summary>
  public class CalcOperation : CalcExpression {
    /// <summary>
    /// The value on the left side of the operator. Is null if the
    ///   <c>Operator</c> is a <c>CLPrefixOperator</c>.
    /// </summary>
    public CalcObject Left { get; }
    /// <summary>
    /// The value on the right side of the operator. Is null if the
    ///   <c>Operator</c> is a <c>CLPostfixOperator</c>.
    /// </summary>
    public CalcObject Right { get; }
    /// <summary>
    /// The operator itself.
    /// </summary>
    public CLOperator Operator { get; }

    public override string ToCode() {
      string left = Left?.ToCode() ?? "";
      string right = Right?.ToCode() ?? "";
      return "(" + left + Operator.Symbol + right + ")";
    }

    public override string ToString(int level) {
      if (level == 0) return "(...)";

      string left = Left?.ToString(level - 1) ?? "";
      string right = Right?.ToString(level - 1) ?? "";

      return "(" + left + Operator.Symbol + right + ")";
    }

    public override CalcValue GetValue(CLLocalStore vars, object context = null) {
      if (Operator is CLBinaryOperator bin) return bin.Run(Left, Right, vars, context);
      else if (Operator is CLPrefixOperator pre) return pre.Run(Right, vars, context);
      else if (Operator is CLPostfixOperator post) return post.Run(Left, vars, context);
      else throw new InvalidCastException("Operators must be binary, prefix, or postfix.");
    }

    public override string ToTree(int level) {
      string ret = new String(' ', level * 2) + "Operation: " + Operator.ToString();

      if (Left != null) ret += "\n" + Left.ToTree(level + 1);
      if (Right != null) ret += "\n" + Right.ToTree(level + 1);

      return ret;
    }
  }
}