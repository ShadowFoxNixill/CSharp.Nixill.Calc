namespace Nixill.CalcLib.Exception
{
  [System.Serializable]
  public class CLSyntaxException : System.Exception
  {
    public int Position { get; }

    public CLSyntaxException() { }
    public CLSyntaxException(string message, int pos) : base(message)
    {
      Position = pos;
    }
    public CLSyntaxException(string message, System.Exception inner, int pos) : base(message, inner)
    {
      Position = pos;
    }
  }
}