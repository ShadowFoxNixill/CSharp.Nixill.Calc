using System;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Functions {
  public class CLCodeFunction {
    public Func<CalcObject[], object, CLLocalStore, CalcValue> FunctionDef { get; private set; }
    public string Name { get; private set; }

    Dictionary<string, CLCodeFunction> AllFunctions = new Dictionary<string, CLCodeFunction>();
    public CLCodeFunction this[string index] => AllFunctions[index];

    public CLCodeFunction(string name, Func<CalcObject[], object, CLLocalStore, CalcValue> funcDef) {
      Name = name;
      FunctionDef = funcDef;

      AllFunctions[name] = this;
    }
  }
}