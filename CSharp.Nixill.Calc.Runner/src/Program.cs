// See https://aka.ms/new-console-template for more information

using Nixill.CalcLib.Modules;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Parsing;

MainModule.LoadBinaryDivide();
MainModule.LoadBinaryMinus();
MainModule.LoadBinaryPlus();
MainModule.LoadBinaryTimes();

CalcObject obj = CLInterpreter.Interpret("(9*(3+2)+25)*9+1");

Console.WriteLine(obj.ToCode());