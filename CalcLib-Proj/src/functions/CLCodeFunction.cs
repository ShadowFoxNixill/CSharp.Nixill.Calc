using System;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;
using Nixill.CalcLib.Varaibles;

namespace Nixill.CalcLib.Functions {
  /// <summary>
  /// Defines the code of a <link cref="CalcCodeFunction">CalcCodeFunction</link>.
  /// </summary>
  public class CLCodeFunction {
    /// <summary>
    /// The function that runs whenever the code function is called.
    /// </summary>
    public Func<CalcObject[], CLLocalStore, object, CalcValue> FunctionDef { get; private set; }
    /// <summary>The name of the code function.</summary>
    public string Name { get; private set; }

    static Dictionary<string, CLCodeFunction> AllFunctions = new Dictionary<string, CLCodeFunction>();

    /// <summary>
    /// Gets a <c>CLCodeFunction</c> by its name.
    /// </summary>
    public static CLCodeFunction Get(string name) => AllFunctions[name];

    /// <summary>
    /// Returns whether a <c>CLCodeFunction</c> exists with the given
    ///   name.
    /// </summary>
    public static bool Exists(string name) => AllFunctions.ContainsKey(name);

    /// <summary>Creates a new <c>CLCodeFunction</c>.</summary>
    /// <param name="name">The name to give the function.</param>
    /// <param name="funcDef">The code of the function.</param>
    /// <remarks>
    /// <para>Any existing functions with the same name are replaced, but
    ///   existing
    ///   <link cref="CalcCodeFunction"><c>CalcCodeFunction</c></link>s
    ///   will continue to reference the old <c>CLCodeFunction</c>.</para>
    public CLCodeFunction(string name, Func<CalcObject[], CLLocalStore, object, CalcValue> funcDef) {
      Name = name;
      FunctionDef = funcDef;

      AllFunctions[name] = this;
    }
  }
}