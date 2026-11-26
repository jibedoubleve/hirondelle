using Hirondelle.Scripts;
using Shouldly;
using Xunit;

namespace Hirondelle.Tests;

public class ScriptTest
{
    #region Methods

    [Theory]
    [InlineData("create_tables.sql")]
    [InlineData("1.1.0_add_users.txt")]
    [InlineData("")]
    [InlineData("1.0_create_tables.sql")]
    [InlineData("1.0.0.0.0_create_tables.sql")]
    [InlineData("1.0.0_create.tables.sql")]
    [InlineData("1.0.0_.sql")]
    [InlineData("99999999999.0.0_x.sql")]
    [InlineData(" 1.0.0_create_tables.sql")]
    [InlineData("1.0.0_create_tables.sql ")]
    [InlineData("1.0.0_create tables.sql")]
    public void When_parsing_invalid_script_name_Then_exception_is_thrown(string scriptName)
    {
        // act
        Action act = () => ScriptInfo.Parse(scriptName);

        // assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void When_parsing_null_script_name_Then_ArgumentNullException_is_thrown()
    {
        // act
        Action act = () => ScriptInfo.Parse(null!);

        // assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [InlineData("1.0.0_create_tables.sql", "1.0.0", "create_tables")]
    [InlineData("1.1.0_add_users.sql", "1.1.0", "add_users")]
    [InlineData("1.1.0.1_seed_users.sql", "1.1.0.1", "seed_users")]
    [InlineData("1.2.0_add_index.sql", "1.2.0", "add_index")]
    [InlineData("1.2.0.sql", "1.2.0", "")]
    [InlineData("1.3.0-add_roles.sql", "1.3.0", "add_roles")]
    [InlineData("1.0.0_CREATE_TABLES.SQL", "1.0.0", "CREATE_TABLES")]
    [InlineData("1.0.0_CREATE_TABLES.sql", "1.0.0", "CREATE_TABLES")]
    [InlineData("01.02.003_create_tables.sql", "1.2.3", "create_tables")]
    [InlineData("1.1.0.1-seed_users.sql", "1.1.0.1", "seed_users")]
    public void When_parsing_valid_script_name_Then_ScriptInfo_has_expected_information(
        string scriptName, string versionStr, string name)
    {
        // arrange
        var version = new Version(versionStr);

        // act
        var scriptInfo = ScriptInfo.Parse(scriptName);

        // assert
        scriptInfo.ShouldSatisfyAllConditions(
            info => info.Name.ShouldBe(name),
            info => info.Version.ShouldBe(version)
        );
    }

    #endregion
}