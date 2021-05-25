namespace Nixill.CalcLib.Exception {
  [System.Serializable]
  public class CLSyntaxException : System.Exception {
    public int Position { get; }

    public CLSyntaxException() { }
    public CLSyntaxException(string message, int pos) : base(message) {
      Position = pos;
    }
    public CLSyntaxException(string message, System.Exception inner, int pos) : base(message, inner) {
      Position = pos;
    }
    protected CLSyntaxException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }
}