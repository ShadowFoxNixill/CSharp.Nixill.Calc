using System;
using System.Collections.Generic;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;

namespace Nixill.CalcLib.Varaibles {
  public static class CLVariables {
    public static event EventHandler<CLVariableLoad> VariableLoaded = (sender, pars) => { };
    public static event EventHandler<CLVariableSave> VariableSaved = (sender, pars) => { };

    private static Dictionary<string, CalcObject> InternalStorage = new Dictionary<string, CalcObject>();

    public static CalcObject Load(string name, object context) {
      CLVariableLoad data = new CLVariableLoad() {
        Name = name
      };

      VariableLoaded.Invoke(context, data);

      if (data.Value == null) {
        try {
          return InternalStorage[name];
        }
        catch (KeyNotFoundException e) {
          throw new CalcException(e);
        }
      }
      else return data.Value;
    }

    public static void Save(string name, CalcObject val, object context) {
      CLVariableSave data = new CLVariableSave() {
        Name = name,
        Value = val
      };

      VariableSaved.Invoke(context, data);

      if (!data.Saved) InternalStorage[name] = val;
    }
  }

  public class CLVariableLoad {
    public string Name { get; internal set; }
    public CalcObject Value { internal get; set; }
    public bool IsValueSet => Value != null;
  }

  public class CLVariableSave {
    public string Name { get; internal set; }
    public CalcObject Value { get; internal set; }
    public bool Saved = false;
  }
}