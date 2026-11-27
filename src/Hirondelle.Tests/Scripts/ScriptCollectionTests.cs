using Hirondelle.Scripts;
using Hirondelle.Tests.Utils;
using Shouldly;
using Xunit;

namespace Hirondelle.Tests.Scripts;

public class ScriptCollectionTests
{
    #region Methods

    [Fact]
    public void When_building_collection_with_null_Then_exception_is_thrown()
    {
        // act
        var sut = () => new ScriptCollection(null!);

        // assert
        sut.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void When_creating_script_collection_Then_scripts_are_sorted_by_ascending_version()
    {
        // arrange
        IEnumerable<string> versions =
        [
            "1.0.0",
            "0.9.3",
            "0.9.0",
            "0.9.6",
            "0.8.0"
        ];
        IEnumerable<string> sorted =
        [
            "0.8.0.0",
            "0.9.0.0",
            "0.9.3.0",
            "0.9.6.0",
            "1.0.0.0"
        ];

        const string name = "create_tables.sql";
        var scripts = versions.Select(Version.Parse)
                              .Select(ver => ScriptInfo.Parse($"{ver}_{name}"))
                              .ToList();

        // act
        var collection = new ScriptCollection(scripts.ToScripts());

        // assert
        collection.Select(e => e.Info.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0")]
    [InlineData("1.0.0", "1.0.0.0")]
    [InlineData("01.00.00", "1.0.0.0")]
    public void When_versions_are_duplicated_Then_exception_is_thrown(string first, string second)
    {
        // arrange
        const string name = "create_tables.sql";
        var scripts = new List<ScriptInfo>
        {
            ScriptInfo.Parse($"{first}_{name}"),
            ScriptInfo.Parse($"{second}_{name}")
        };

        // act
        var sut = () => new ScriptCollection(scripts.ToScripts());

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_duplicated_versions_have_different_names_Then_exception_is_thrown()
    {
        // arrange
        string[] strings = ["1.0.0_a.sql", "1.0.0-b.sql"];
        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var sut = () => new ScriptCollection(scripts.ToScripts());

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_versions_have_3_and_4_parts_Then_scripts_are_sorted_by_ascending_version()
    {
        // arrange
        string[] strings =
        [
            "1.0.0_create_tables.sql",
            "1.1.0_add_users.sql",
            "1.1.0.1_seed_users.sql",
            "1.2.0_add_index.sql"
        ];
        IEnumerable<string> sorted =
        [
            "1.0.0.0",
            "1.1.0.0",
            "1.1.0.1",
            "1.2.0.0"
        ];

        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var collection = new ScriptCollection(scripts.ToScripts());

        // assert
        collection.Select(e => e.Info.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Fact]
    public void When_a_script_has_no_info_Then_exception_is_thrown()
    {
        // arrange
        string[] strings =
        [
            "1.0.0_create_tables.sql",
            "1.1.0.1_seed_users.sql",
            "1.2.0_add_index.sql"
        ];

        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();
        scripts.Add(null!);

        // act
        var collection = () => new ScriptCollection(scripts.ToScripts());

        // assert
        collection.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void When_duplicated_versions_are_not_adjacent_Then_exception_is_thrown()
    {
        // arrange
        string[] strings =
        [
            "1.0.0_create_tables.sql",
            "1.1.0_add_users.sql",
            "1.1.0.1_seed_users.sql",
            "1.1.0_add_index.sql"
        ];

        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var sut = () => new ScriptCollection(scripts.ToScripts());

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_collection_has_a_single_script_Then_it_is_returned()
    {
        // arrange
        string[] strings = ["1.0.0_create_tables.sql"];
        IEnumerable<string> sorted = ["1.0.0.0"];

        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var collection = new ScriptCollection(scripts.ToScripts());

        // assert
        collection.Select(e => e.Info.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Fact]
    public void When_versions_have_different_part_counts_Then_numeric_sort_is_used()
    {
        // arrange
        IEnumerable<string> versions =
        [
            "1.10.0",
            "1.9.0.1",
        ];
        IEnumerable<string> sorted =
        [
            "1.9.0.1",
            "1.10.0.0"
        ];
        const string name = "create_tables.sql";
        var scripts = versions.Select(Version.Parse)
                              .Select(ver => ScriptInfo.Parse($"{ver}_{name}"))
                              .ToList();

        // act
        var collection = new ScriptCollection(scripts.ToScripts());

        // assert
        collection.Select(e => e.Info.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Fact]
    public void When_sorting_scripts_Then_numeric_sort_is_used()
    {
        // arrange
        IEnumerable<string> versions =
        [
            "1.10.0",
            "1.9.0"
        ];
        IEnumerable<string> sorted =
        [
            "1.9.0.0",
            "1.10.0.0"
        ];
        const string name = "create_tables.sql";
        var scripts = versions.Select(Version.Parse)
                              .Select(ver => ScriptInfo.Parse($"{ver}_{name}"))
                              .ToList();

        // act
        var collection = new ScriptCollection(scripts.ToScripts());

        // assert
        collection.Select(e => e.Info.Version.ToString())
                  .ShouldBe(sorted);
    }

    #endregion
}