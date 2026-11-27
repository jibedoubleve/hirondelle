using System.Reflection;
using Hirondelle.Scripts;

namespace Hirondelle.Resources;

internal static class ScriptManager
{
    #region Methods

    private static IEnumerable<Script> FetchScripts(Assembly assembly, Dictionary<string, string> resources)
    {
        foreach (var resource in resources)
        {
            using var stream = assembly.GetManifestResourceStream(resource.Key);

            if (stream is null) { continue; }

            using var reader = new StreamReader(stream);
            
            var sql = reader.ReadToEnd()
                            .ReplaceLineEndings("\n")
                            .Trim();
            
            yield return new Script(
                ScriptInfo.Parse(resource.Value),
                sql
            );
        }
    }

    public static ScriptCollection Fetch(Assembly assembly, string prefix = "Hirondelle")
    {
        prefix = Normalize(prefix);

        var resources = assembly.GetManifestResourceNames()
                                .Where(r => r.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                .Where(r => r.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                                .Select(r => new KeyValuePair<string, string>(r, r[prefix.Length..]))
                                .ToDictionary();

        if (!resources.Any())
        {
            throw new ArgumentException(
                $"No migration script found in assembly '{assembly.FullName}' with prefix '{prefix}'."
            );
        }

        var scripts = FetchScripts(assembly, resources);

        return new ScriptCollection(scripts);

        static string Normalize(string prefix)
        {
            if (!prefix.EndsWith('\\')) { prefix += '\\'; }

            return prefix.Replace('\\', Path.DirectorySeparatorChar)
                         .Replace('/', Path.DirectorySeparatorChar);
        }
    }

    #endregion
}