using System.Text.RegularExpressions;

namespace Hirondelle.Scripts;

internal sealed partial record ScriptInfo
{
    #region Constructors

    private ScriptInfo(Version version, string name)
    {
        Version = version;
        Name = name;
    }

    #endregion

    #region Properties

    public string Name { get; }
    public Version Version { get; }

    #endregion

    #region Methods

    [GeneratedRegex(@"^(?<version>\d+\.\d+\.\d+(?:\.\d+)?)(?:[_-](?<name>[^.\s]+))?\.sql\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NameParser();

    public static ScriptInfo Parse(string scriptName)
    {
        ArgumentNullException.ThrowIfNull(scriptName);

        scriptName = scriptName[(scriptName.LastIndexOfAny(['\\', '/']) + 1)..];
        var matches = NameParser().Matches(scriptName);

        if (matches.Count == 0) { throw new ArgumentException($"Invalid script name: {scriptName}"); }

        var name = matches[0].Groups["name"].Value;
        
        try
        {
            var version = ParseNormalized(matches[0].Groups["version"].Value);

            return new ScriptInfo(version, name);
        }
        catch (Exception ex) { throw new ArgumentException($"Invalid version in script name: {scriptName}", ex); }


        static Version ParseNormalized(string versionString)
        {
            var parsed = Version.Parse(versionString);
            return new Version(
                parsed.Major,
                parsed.Minor,
                Math.Max(parsed.Build, 0),
                Math.Max(parsed.Revision, 0));
        }
    }

    #endregion
}