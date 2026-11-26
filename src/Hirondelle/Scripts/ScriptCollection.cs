using System.Collections;

namespace Hirondelle.Scripts;

internal sealed class ScriptCollection : IEnumerable<ScriptInfo>
{
    #region Fields

    private readonly List<ScriptInfo> _scripts;

    #endregion

    #region Constructors

    public ScriptCollection(IEnumerable<ScriptInfo> scripts)
    {
        scripts = scripts.ToList();
        
        ThrowsIfContainsNull(scripts, nameof(scripts));
        
        _scripts = [.. scripts.OrderBy(e => e.Version)];

        for (var i = 1; i < _scripts.Count; i++)
        {
            var current = _scripts[i].Version;
            var previous = _scripts[i - 1].Version;

            if (current == previous)
            {
                throw new ArgumentException(
                    $"A script with the same version {current} already exists.",
                    nameof(scripts)
                );
            }
        }
    }

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static void ThrowsIfContainsNull(IEnumerable<ScriptInfo> scripts, string scriptsName)
    {
        var hasNull = scripts.Any(e => e is null);
        if (hasNull) { throw new ArgumentNullException(scriptsName, "Collection contains null scripts"); }
    }

    public IEnumerator<ScriptInfo> GetEnumerator() => _scripts.GetEnumerator();

    #endregion
}