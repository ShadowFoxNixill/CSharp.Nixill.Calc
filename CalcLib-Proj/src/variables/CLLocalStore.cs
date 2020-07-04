using System.Linq;
using System.Collections.Generic;
using Nixill.CalcLib.Objects;

namespace Nixill.CalcLib.Varaibles {
  internal class CLLocalStore {
    private Dictionary<string, CalcObject> Vars = new Dictionary<string, CalcObject>();
    private List<CalcObject> Params = new List<CalcObject>();

    public CalcObject this[string index] { get => Vars[index]; set => Vars[index] = value; }
    public CalcObject this[int index] {
      get => Params[index];
      set {
        if (index < Params.Count) Params[index] = value;
        else Params.Add(value);
      }
    }

    public CLLocalStore() { }

    public CLLocalStore(List<CalcObject> pars) {
      Params = new List<CalcObject>(pars);
    }

    public CLLocalStore(Dictionary<string, CalcObject> vars) {
      Vars = new Dictionary<string, CalcObject>(vars);
    }

    public CLLocalStore(List<CalcObject> pars, Dictionary<string, CalcObject> vars) {
      Params = new List<CalcObject>(pars);
      Vars = new Dictionary<string, CalcObject>(vars);
    }

    public CLLocalStore(CalcObject[] pars) {
      Params = new List<CalcObject>(pars);
    }

    public CLLocalStore(CalcObject[] pars, Dictionary<string, CalcObject> vars) {
      Params = new List<CalcObject>(pars);
      Vars = new Dictionary<string, CalcObject>(vars);
    }

    public bool ContainsVar(string name) => Vars.ContainsKey(name);
    public int VarCount => Vars.Count;
    public int ParamCount => Params.Count;
  }
}