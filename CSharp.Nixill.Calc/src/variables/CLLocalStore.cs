using System.Collections.Generic;
using Nixill.CalcLib.Objects;

namespace Nixill.CalcLib.Varaibles {
  /// <summary>
  /// A storage for local-scoped variables.
  /// </summary>
  public class CLLocalStore {
    private Dictionary<string, CalcObject> Vars = new Dictionary<string, CalcObject>();
    private List<CalcObject> Params = new List<CalcObject>();

    /// <summary>
    /// Whether or not the storage contains a specific variable.
    /// </summary>
    /// <param name="name">The name of variable to find.</param>
    public bool ContainsVar(string name) => Vars.ContainsKey(name);

    /// <summary>How many variables this storage contains.</summary>
    public int VarCount => Vars.Count;

    /// <summary>
    /// How many function parameters this storage contains.
    /// </summary>
    public int ParamCount => Params.Count;

    /// <summary>
    /// Get or set a variable in this storage.
    /// </summary>
    public CalcObject this[string index] { get => Vars[index]; set => Vars[index] = value; }

    /// <summary>
    /// Get or set a parameter in this storage.
    /// </summary>
    public CalcObject this[int index] {
      get => Params[index];
      set {
        if (index < Params.Count) Params[index] = value;
        else Params.Add(value);
      }
    }

    /// <summary>
    /// Gets the whole set of parameters in storage.
    /// </summary>
    public CalcObject[] CopyParams() => Params.ToArray();

    /// <summary>
    /// Creates an empty <c>CLLocalStore</c>.
    /// </summary>
    public CLLocalStore() { }

    /// <summary>
    /// Creates a <c>CLLocalStore</c> with existing parameters.
    /// </summary>
    public CLLocalStore(List<CalcObject> pars) {
      Params = new List<CalcObject>(pars);
    }

    /// <summary>
    /// Creates a <c>CLLocalStore</c> with existing variables.
    /// </summary>
    public CLLocalStore(Dictionary<string, CalcObject> vars) {
      Vars = new Dictionary<string, CalcObject>(vars);
    }

    /// <summary>
    /// Creates a <c>CLLocalStore</c> with existing parameters and
    ///   existing variables.
    /// </summary>
    public CLLocalStore(List<CalcObject> pars, Dictionary<string, CalcObject> vars) {
      Params = new List<CalcObject>(pars);
      Vars = new Dictionary<string, CalcObject>(vars);
    }

    /// <summary>
    /// Creates a <c>CLLocalStore</c> with existing parameters.
    /// </summary>
    public CLLocalStore(CalcObject[] pars) {
      Params = new List<CalcObject>(pars);
    }

    /// <summary>
    /// Creates a <c>CLLocalStore</c> with existing parameters and
    ///   existing variables.
    /// </summary>
    public CLLocalStore(CalcObject[] pars, Dictionary<string, CalcObject> vars) {
      Params = new List<CalcObject>(pars);
      Vars = new Dictionary<string, CalcObject>(vars);
    }
  }
}