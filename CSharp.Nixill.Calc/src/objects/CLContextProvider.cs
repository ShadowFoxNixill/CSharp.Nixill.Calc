using System;
using System.Collections.Generic;
using Nixill.Utils;

namespace Nixill.CalcLib.Objects {
  /// <summary>
  /// A provider of context for operators and functions that require
  ///   additional outside context to work (such as a username).
  /// </summary>
  public class CLContextProvider {
    private Dictionary<Type, object> Contexts;

    /// <summary>Creates a new <c>CLContextProvider</c>.</summary>
    public CLContextProvider() {
      Contexts = new Dictionary<Type, object>();
    }

    /// <summary>Adds context to this <c>CLContextProvider</c>.</summary>
    /// <remarks>
    /// If the <c>CLContextProvider</c> already contained an object of
    ///   this type, it is overridden.
    /// </remarks>
    public void Add(object obj) {
      Contexts[obj.GetType()] = obj;
    }

    /// <summary>
    /// Returns whether or not this <c>CLContextProvider</c> contains
    ///   context of the given type.
    /// </summary>
    public bool Contains(Type type) {
      return Contexts.ContainsKey(type);
    }

    /// <summary>
    /// Returns whether or not this <c>CLContextProvider</c> contains
    ///   context of the given type, or a type derived from it.
    /// </summary>
    /// <param name="type">The type to look for.</param>
    /// <param name="derived">
    /// The found type, or <c>null</c> if none found.
    /// </param>
    public bool ContainsDerived(Type type, out Type derived) {
      if (Contains(type)) {
        derived = type;
        return true;
      }

      foreach (Type contained in Contexts.Keys) {
        if (contained.IsSubclassOf(type)) {
          derived = contained;
          return true;
        }
      }

      derived = null;
      return false;
    }

    /// <summary>
    /// Returns the context of the given type.
    /// </summary>
    public object Get(Type type) {
      return Contexts[type];
    }
  }
}