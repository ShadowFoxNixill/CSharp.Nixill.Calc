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
  }
}