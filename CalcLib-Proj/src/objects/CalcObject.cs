namespace Nixill.CalcLib.Objects {
  public abstract class CalcObject {
    public abstract CalcValue GetValue();
    public abstract override string ToString();
    public abstract string ToCode();
  }
}