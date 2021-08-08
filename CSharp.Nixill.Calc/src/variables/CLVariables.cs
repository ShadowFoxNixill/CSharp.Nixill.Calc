using System;
using System.Collections.Generic;
using Nixill.CalcLib.Exception;
using Nixill.CalcLib.Objects;

namespace Nixill.CalcLib.Varaibles {
  /// <summary>
  /// Class that handles saving and loading of non-local variables.
  /// </summary>
  public static class CLVariables {
    /// <summary>
    /// Fired when an expression attempts to load a variable.
    /// </summary>
    public static event EventHandler<CLVariableLoad> VariableLoaded = (sender, pars) => { };
    /// <summary>
    /// Fired when an expression attempts to save a variable.
    /// </summary>
    public static event EventHandler<CLVariableSave> VariableSaved = (sender, pars) => { };
    /// <summary>
    /// Fired when an expression attempts to delete a variable.
    /// </summary>
    public static event EventHandler<CLVariableDelete> VariableDeleted = (sender, pars) => { };

    private static Dictionary<string, CalcObject> InternalStorage = new Dictionary<string, CalcObject>();

    /// <summary>
    /// Loads a variable and returns the <c>CalcObject</c> it represents.
    /// </summary>
    /// <param name="name">The name of the variable to load.</param>
    /// <param name="context">
    /// An object representing the context in which it's run.
    /// </param>
    public static CalcObject Load(string name, CLContextProvider context) {
      CLVariableLoad data = new CLVariableLoad() {
        Name = name
      };

      VariableLoaded.Invoke(context, data);

      if (data.Value == null) {
        try {
          return InternalStorage[name];
        }
        catch (KeyNotFoundException e) {
          throw new CLException(e);
        }
      }
      else return data.Value;
    }

    /// <summary>
    /// Saves a <c>CalcObject</c> as a variable.
    /// </summary>
    /// <param name="name">The name of the variable to save.</param>
    /// <param name="val">The value to save.</param>
    /// <param name="context">
    /// An object representing the context in which it's run.
    /// </param>
    public static void Save(string name, CalcObject val, CLContextProvider context) {
      CLVariableSave data = new CLVariableSave() {
        Name = name,
        Value = val
      };

      VariableSaved.Invoke(context, data);

      if (!data.Saved) InternalStorage[name] = val;
    }

    /// <summary>
    /// Deletes a saved variable.
    /// </summary>
    /// <param name="name">The name of the variable to delete.</param>
    /// <param name="contet">
    /// An object representing the context in which it's run.
    /// </param>
    public static void Delete(string name, CLContextProvider context) {
      CLVariableDelete data = new CLVariableDelete() {
        Name = name
      };

      VariableDeleted.Invoke(context, data);

      if (!data.Deleted) InternalStorage.Remove(name);
    }
  }

  /// <summary>Event data for when a variable loads.</summary>
  public class CLVariableLoad {
    /// <summary>The name of the variable to load.</summary>
    public string Name { get; internal set; }
    /// <summary>The value of the loaded variable.</summary>
    public CalcObject Value { internal get; set; }
    /// <summary>Returns true iff a value is set.</summary>
    public bool IsValueSet => Value != null;
  }

  /// <summary>Event data for when a variable saves.</summary>
  public class CLVariableSave {
    /// <summary>The name of the variable to save.</summary>
    public string Name { get; internal set; }
    /// <summary>The value to save as the variable.</summary>
    public CalcObject Value { get; internal set; }
    /// <summary>Set this to true when the variable is saved.</summary>
    public bool Saved = false;
  }

  /// <summary>Event data for when a variable is deleted.</summary>
  public class CLVariableDelete {
    /// <summary>The name of the variable to delete.</summary>
    public string Name { get; internal set; }
    /// <summary>Set this to true when the variable is deleted.</summary>
    public bool Deleted = false;
  }
}