using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Objects {
  public abstract class CalcObject {
    public CalcValue GetValue() => GetValue(new CLLocalStore());
    internal abstract CalcValue GetValue(CLLocalStore store);
    public sealed override string ToString() => ToString(1);
    public abstract string ToString(int level);
    public abstract string ToCode();
    public abstract override bool Equals(object other);
    public abstract override int GetHashCode();

    public static bool operator ==(CalcObject left, CalcObject right) => left.Equals(right);
    public static bool operator !=(CalcObject left, CalcObject right) => !(left.Equals(right));
  }
}