using Nixill.CalcLib.Functions;

namespace Nixill.CalcLib.Modules {
  public class MathModule {
    public static bool Loaded { get; private set; } = false;

    public static CLCodeFunction E { get; private set; }
    public static CLCodeFunction PI { get; private set; }

    public static CLCodeFunction Abs { get; private set; }
    public static CLCodeFunction Acos { get; private set; }
    public static CLCodeFunction Acosh { get; private set; }
    public static CLCodeFunction Asin { get; private set; }
    public static CLCodeFunction Asinh { get; private set; }
    public static CLCodeFunction Atan { get; private set; }
    public static CLCodeFunction Atan2 { get; private set; }
    public static CLCodeFunction Atanh { get; private set; }
  }
}