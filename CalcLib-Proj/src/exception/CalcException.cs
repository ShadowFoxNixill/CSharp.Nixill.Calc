using System;

namespace Nixill.CalcLib.Exception {
  /// <summary>
  /// An exception thrown when a calculation errors.
  /// </summary>
  public class CLException : System.Exception {
    /// <summary>
    /// Creates a CalcException with a message.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    public CLException(string message) : base(message) { }

    /// <summary>
    /// Creates a CalcException with a base exception and an optional
    /// message.
    /// </summary>
    /// <param name="basex">The exception that caused this
    ///   <c>CalcException</c>.</param>
    /// <param name="message">The message to be displayed.</param>
    public CLException(SystemException basex, string message = "") : base(message, basex) { }
  }
}