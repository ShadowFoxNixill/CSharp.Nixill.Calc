using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcObject {
    /// <summary>
    /// Evaluates this <c>CalcObject</c> and returns its value.
    /// </summary>
    /// <param name="vars">A <c>CLLocalStore</c> that stores local
    ///   variables.</param>
    /// <param name="context">The object representing the context in which
    ///   the expression is being evaluated.</param>
    public abstract CalcValue GetValue(CLLocalStore store = null, object context = null);

    /// <summary>
    /// Returns whether two <c>CalcObject</c>s are the same.
    ///   (<c>CalcObject</c>s are not equal to objects that are not
    ///   <c>CalcObject</c>s.)
    /// </summary>
    /// <param name="other">The object to compare this to.</param>
    public abstract override bool Equals(object other);

    /// <summary>
    /// Returns the hash code of the <c>CalcObject</c>.
    /// </summary>
    public abstract override int GetHashCode();

    /// <summary>
    /// Returns a string representation of this <c>CalcObject</c> that
    ///   can be parsed back into another copy of the <c>CalcObject</c>.
    /// </summary>
    public abstract string ToCode();

    /// <summary>
    /// Returns the string representation of this <c>CalcObject</c> at
    ///   level 1.
    /// </summary>
    public sealed override string ToString() => ToString(1);

    /// <summary>
    /// Returns the string representation of this <c>CalcObject</c>.
    /// </summary>
    /// <param name="level">What level to print out. Level <c>0</c>
    ///   shouldn't print any children; other levels should print children
    ///   at one level lower.</param>
    public abstract string ToString(int level);

    /// <summary>
    /// Returns a detailed tree of this <c>CalcObject</c> and all its
    ///   children.
    /// </summary>
    /// <param name="level">
    /// The level at which to print this object.
    /// </param>
    public abstract string ToTree(int level);

    /// <summary>
    /// Returns whether two <c>CalcObject</c>s are equal.
    /// </summary>
    /// <param name="left">The left <c>CalcObject</c>.</param>
    /// <param name="right">The right <c>CalcObject</c>.</param>
    public static bool operator ==(CalcObject left, CalcObject right) => object.Equals(left, right);

    /// <summary>
    /// Returns whether two <c>CalcObject</c>s are inequal.
    /// </summary>
    /// <param name="left">The left <c>CalcObject</c>.</param>
    /// <param name="right">The right <c>CalcObject</c>.</param>
    public static bool operator !=(CalcObject left, CalcObject right) => !object.Equals(left, right);
  }
}