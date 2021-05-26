using NUnit.Framework;

using Nixill.CalcLib.Modules;
using Nixill.CalcLib.Parsing;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.Test {
  public class CalcLibTests {
    [Test]
    public void TestV1() {
      MainModule.Load();

      // The first tests
      TestLine("3");
      TestLine("4+2");
      TestLine("3/-2");
      TestLine("2*(-1+5)");
      TestLine("2+[1,2,3]");
      TestLine("[1,(1+1),(1-1)]");

      // More tests
      CLVariables.VariableLoaded += Loader.Load;
      TestLine("2^5");
      TestLine("{two}^{four}");
      TestLine("{level,3}");
      TestLine("{level,10}");
      TestLine("{level}");

      // Math module tests
      MathModule.Load();
      TestLine("{!abs,-4}");
      TestLine("{!sin,1}");
      TestLine("{!max,2,-3}");

      // Decimal testing
      TestLine(".4");
      TestLine("4*.5");
    }

    public void TestLine(string input, bool resolve = true) {
      // First, let's parse it
      CalcObject obj1 = CLInterpreter.Interpret(input);

      // And put it back to string and back
      string code2 = obj1.ToCode();
      CalcObject obj3 = CLInterpreter.Interpret(code2);
      string code4 = obj3.ToCode();

      // code2 and code4 need to match
      Assert.AreEqual(code2, code4, "code2 and code4 don't match on line: " + input);

      // We'll stop here if we're testing non-deterministic functions
      if (!resolve) return;

      // Then, let's get the value
      CalcValue val5 = obj1.GetValue();
      string code6 = val5.ToCode();
      CalcObject obj7 = CLInterpreter.Interpret(code6);
      string code8 = obj7.ToCode();
      CalcValue val9 = obj7.GetValue();

      // code6 and code8 need to match
      Assert.AreEqual(code6, code8, "code6 and code8 don't match on line: " + input);

      // val5 and val9 also need to match
      Assert.AreEqual(val5, val9, "val5 and val9 don't match on line: " + input);
    }
  }

  public class Loader {
    private static CalcObject Level = CLInterpreter.Interpret("({1,5}^2+{1,5})/2");
    private static CalcObject Four = new CalcNumber(4);
    private static CalcObject Two = new CalcNumber(2);

    public static void Load(object sender, CLVariableLoad args) {
      string name = args.Name;

      if (name == "level") args.Value = Level;
      if (name == "four") args.Value = Four;
      if (name == "two") args.Value = Two;
    }
  }
}