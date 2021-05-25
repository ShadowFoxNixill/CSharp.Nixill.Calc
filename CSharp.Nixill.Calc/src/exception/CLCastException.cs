using System.Collections;
using System.Runtime.Serialization;

namespace Nixill.CalcLib.Exception {
  public class CLCastException : CLException {
    public CLCastException(string message) : base(message) { }
    public CLCastException(string message, System.Exception inner) : base(inner, message) { }
  }
}