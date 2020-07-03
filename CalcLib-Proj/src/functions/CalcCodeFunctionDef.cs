using System;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;

namespace Nixill.CalcLib.Functions {
  public class CalcCodeFunctionDef {
    public Func<List<CalcObject>, CalcValue> FunctionDef { get; private set; }
    public string Name { get; private set; }

    Dictionary<string, CalcCodeFunctionDef> AllFunctions = new Dictionary<string, CalcCodeFunctionDef>();
    public CalcCodeFunctionDef this[string index] => AllFunctions[index];

    public CalcCodeFunctionDef(string name, Func<List<CalcObject>, CalcValue> funcDef) {
      Name = name;
      FunctionDef = funcDef;

      AllFunctions[name] = this;
    }
  }
}