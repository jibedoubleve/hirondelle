using System.Collections;

namespace Hirondelle.Scripts;

internal sealed class ScriptCollection : IEnumerable<Script>
{
    #region Fields

    private readonly List<Script> _scripts;

    #endregion

    #region Constructors

    public ScriptCollection(IEnumerable<Script> scripts)
    {
        ArgumentNullException.ThrowIfNull(scripts);
        scripts = scripts.ToList();

        ThrowsIfContainsNull(scripts, nameof(scripts));

        _scripts = [.. scripts.OrderBy(e => e.Info.Version)];

        for (var i = 1; i < _scripts.Count; i++)
        {
            var current = _scripts[i].Info.Version;
            var previous = _scripts[i - 1].Info.Version;

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

    #region Properties

    public int Count => _scripts.Count;

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static void ThrowsIfContainsNull(IEnumerable<Script> scripts, string scriptsName)
    {
        var hasNull = scripts.Any(e => e?.Info is null);
        if (hasNull) { throw new ArgumentNullException(scriptsName, "Collection contains null scripts"); }
    }

    public IEnumerator<Script> GetEnumerator() => _scripts.GetEnumerator();

    #endregion
}