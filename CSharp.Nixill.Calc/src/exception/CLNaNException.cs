namespace Nixill.CalcLib.Exception {
  public class CLNaNException : CLException {
    public CLNaNException(string message) : base(message) { }
    public CLNaNException(string message, System.Exception inner) : base(inner, message) { }
  }
}