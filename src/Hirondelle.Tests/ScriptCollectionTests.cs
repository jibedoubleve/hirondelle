using Hirondelle.Scripts;
using Shouldly;
using Xunit;

namespace Hirondelle.Tests;

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
        var collection = new ScriptCollection(scripts);

        // assert
        collection.Select(e => e.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0")]
    [InlineData("1.0.0", "1.0.0.0")]
    [InlineData("01.00.00", "1.0.0.0")]
    public void When_having_doubloons_Then_exception_is_thrown(string first, string second)
    {
        // arrange
        const string name = "create_tables.sql";
        var scripts = new List<ScriptInfo>
        {
            ScriptInfo.Parse($"{first}_{name}"),
            ScriptInfo.Parse($"{second}_{name}")
        };

        // act
        var sut = () => new ScriptCollection(scripts);

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_having_doubloons_with_different_names_Then_exception_is_thrown()
    {
        // arrange
        string[] strings = ["1.0.0_a.sql", "1.0.0-b.sql"];
        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var sut = () => new ScriptCollection(scripts);

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_parsing_scripts_Then_scripts_are_sorted_by_ascending_version()
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
        var collection = new ScriptCollection(scripts);

        // assert
        collection.Select(e => e.Version.ToString())
                  .ShouldBe(sorted);
    }
    
    [Fact]
    public void When_null_scripts_in_the_list_Then_exception_is_thrown()
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
        var collection = () => new ScriptCollection(scripts);

        // assert
        collection.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void When_parsing_scripts_with_not_following_doubloons_Then_exception_is_thrown()
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
        var sut = () => new ScriptCollection(scripts);

        // assert
        sut.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_parsing_unique_script_Then_collection_with_one_element_returned()
    {
        // arrange
        string[] strings = ["1.0.0_create_tables.sql"];
        IEnumerable<string> sorted = ["1.0.0.0"];

        var scripts = strings.Select(ScriptInfo.Parse)
                             .ToList();

        // act
        var collection = new ScriptCollection(scripts);

        // assert
        collection.Select(e => e.Version.ToString())
                  .ShouldBe(sorted);
    }

    [Fact]
    public void When_sorting_3_and_4_section_version_Then_scripts_are_sorted_by_ascending_version()
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
        var collection = new ScriptCollection(scripts);

        // assert
        collection.Select(e => e.Version.ToString())
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
        var collection = new ScriptCollection(scripts);

        // assert
        collection.Select(e => e.Version.ToString())
                  .ShouldBe(sorted);
    }

    #endregion
}