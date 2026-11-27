using Hirondelle.Scripts;

namespace Hirondelle.Tests.Utils;

public static class ScriptExtensions
{
    #region Methods

    internal static IEnumerable<Script> ToScripts(this IEnumerable<ScriptInfo> scripts) 
        => scripts.Select(scriptInfo => new Script(scriptInfo, string.Empty));

    #endregion
}