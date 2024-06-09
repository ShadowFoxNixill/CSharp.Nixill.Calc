// See https://aka.ms/new-console-template for more information

using Nixill.CalcLib.Modules;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Parsing;

MainModule.LoadBinaryDivide();
MainModule.LoadBinaryMinus();
MainModule.LoadBinaryPlus();
// MainModule.LoadBinaryTimes();

CalcObject obj = CLInterpreter.Interpret("(75*4+10*(7-2)-1)");

Console.WriteLine(obj.ToCode());
Console.WriteLine(obj.GetValue());