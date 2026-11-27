using System.Reflection;
using Hirondelle.Resources;
using Shouldly;
using Xunit;

namespace Hirondelle.Tests.Resources;

public class ScriptManagerTests
{
    #region Methods

    [Fact]
    public void When_a_script_has_leading_or_trailing_white_characters_Then_they_are_removed()
    {
        // arrange & act
        var scripts = ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Valid"
        );

        // assert
        const string sql = "Not empty";
        scripts.ShouldAllBe(s => s.Sql == sql);
    }

    [Fact]
    public void When_a_script_name_is_invalid_Then_exception_names_the_script()
    {
        // arrange & act
        var scripts = () => ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Invalid"
        );

        // assert
        scripts.ShouldThrow<ArgumentException>()
               .Message.ShouldContain("0.X.1_Fix_DDL.sql");
    }

    [Fact]
    public void When_fetching_resources_Then_scripts_in_subdirectories_are_included()
    {
        // arrange & act
        var scripts = ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Valid"
        );

        // assert
        scripts.Count.ShouldBe(5);
    }

    [Fact]
    public void When_fetching_resource_Then_scripts_are_sorted_by_version_including_subdirectories()
    {
        // arrange & act
        var scripts = ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Valid"
        );

        // assert
        scripts.Select(e => e.Info.Version.ToString())
               .ShouldBe(["0.1.0.0", "0.1.1.0", "0.1.8.0", "1.0.0.0", "1.0.0.1"]);
    }

    [Theory]
    [InlineData(@"Hirondelle\Migrations\DoesNotExist")]
    [InlineData(@"Hirondelle\Migrations\Inval")] // Prefix of an existing folder must not match it
    public void When_no_resource_matches_prefix_Then_exception_is_thrown(string prefix)
    {
        // arrange & act
        var scripts = () => ScriptManager.Fetch(Assembly.GetExecutingAssembly(), prefix);

        // assert
        scripts.ShouldThrow<ArgumentException>()
               .Message.ShouldContain("No migration script found");
    }

    [Fact]
    public void When_same_version_in_different_subdirectories_Then_exception_is_thrown()
    {
        // arrange & act
        var scripts = () => ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Duplicate"
        );

        // assert
        scripts.ShouldThrow<ArgumentException>()
               .Message.ShouldContain("same version");
    }

    [Fact]
    public void When_extension_is_uppercase_Then_script_is_fetched()
    {
        // arrange & act
        var scripts = ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Mixed"
        );

        // assert
        scripts.Select(e => e.Info.Name).ShouldContain("upper");
    }

    [Fact]
    public void When_resource_is_not_a_sql_file_Then_it_is_ignored()
    {
        // arrange & act
        var scripts = ScriptManager.Fetch(
            Assembly.GetExecutingAssembly(),
            @"Hirondelle\Migrations\Mixed"
        );

        // assert
        scripts.Select(e => e.Info.Name)
               .ShouldBe(["lower", "upper"]);
    }

    #endregion
}