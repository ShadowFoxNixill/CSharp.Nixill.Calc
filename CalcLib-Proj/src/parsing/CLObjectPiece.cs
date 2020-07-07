namespace Nixill.CalcLib.Parsing {
  /// <summary>
  /// Represents a single piece of a <c>CalcObject</c>.
  /// </summary>
  public struct CLObjectPiece {
    /// <summary>The text of the piece.</summary>
    public string Contents { get; }
    /// <summary>What type the piece is.</summary>
    public CLObjectPieceType Type { get; }
    /// <summary>
    /// The index of the first character of the contents of the piece.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Creates a new <c>CLObjectPiece</c>.
    /// </summary>
    public CLObjectPiece(string contents, CLObjectPieceType type, int position) {
      Contents = contents;
      Type = type;
      Position = position;
    }
  }

  /// <summary>
  /// Represents the types of pieces of an <c>CalcObject</c>.
  /// </summary>
  public enum CLObjectPieceType {
    /// <summary>
    /// A number, made from just digits and a decimal point.
    /// </summary>
    Number,
    /// <summary>
    /// Any of the symbols <c>}</c>, <c>()</c>, <c>[]</c>, or <c>,</c>.
    /// </summary>
    Spacer,
    /// <summary>Any <c>CLPrefixOperator</c>.</summary>
    PrefixOperator,
    /// <summary>Any <c>CLBinaryOperator</c>.</summary>
    BinaryOperator,
    /// <summary>Any <c>CLPostfixOperator</c>.</summary>
    PostfixOperator,
    /// <summary>A <c>{</c> followed by a name.</summary>
    FunctionName,
    /// <summary>A string, including its surrounding quotes.</summary>
    String,
    /// <summary>
    /// A comment including the opening <c>{%</c> and closing <c>}</c>.
    /// </summary>
    Comment
  }
}