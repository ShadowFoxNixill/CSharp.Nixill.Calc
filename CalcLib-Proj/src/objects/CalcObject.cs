namespace Nixill.CalcLib.Objects {
  public abstract class CalcObject {
    public abstract CalcValue GetValue();
    public sealed override string ToString() => ToString(1);
    public abstract string ToString(int level);
    public abstract string ToCode();
  }
}