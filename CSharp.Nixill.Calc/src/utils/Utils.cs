using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nixill.Utils {
  internal class CLUtils {
    /// <summary>
    /// Returns a <c>List</c> containing just the parameter.
    /// </summary>
    /// <param name="item">The item to include in the <c>List</c>.</param>
    public static List<R> ListOfOne<R>(R item) {
      List<R> ret = new List<R>();
      ret.Add(item);
      return ret;
    }

    /// <summary>
    /// Tests whether a string is a match for a regex pattern. If so, puts
    ///   the details about that match in <c>match</c>.
    /// </summary>
    /// <param name="pattern">The pattern to check against.</param>
    /// <param name="test">The string to test.</param>
    /// <param name="match">Stores information about the match.</param>
    /// <returns>
    /// <c>true</c> iff <c>test</c> matches <c>pattern</c>.
    /// </returns>
    public static bool RegexMatches(Regex pattern, string test, out Match match) {
      match = pattern.Match(test);
      return match.Success;
    }

    /// <summary>
    /// Checks if a given class is either the same as or a subclass of
    ///   another type.
    /// </summary>
    /// <param name="potentialAncestor">
    /// The type to check as a potential ancestor.
    /// </param>
    /// <param name="potentialDescendant">
    /// The type to check as a potential descendant.
    /// </param>
    /// <returns>
    /// <c>true</c> iff <c>potentialDescendant</c> is equal to or a
    ///   subclass of <c>potentialAncestor</c>.
    /// </returns>
    public static bool IsSameOrSubclass(Type potentialAncestor, Type potentialDescendant) {
      return potentialDescendant == potentialAncestor
           || potentialDescendant.IsSubclassOf(potentialAncestor);
    }
  }
}