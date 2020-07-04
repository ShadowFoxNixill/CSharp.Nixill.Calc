using System;

namespace Nixill.CalcLib.Exception {
  public class CalcException : System.Exception {
    public CalcException(string message) : base(message) { }

    public CalcException(SystemException basex, string message = "") : base(message, basex) { }
  }
}