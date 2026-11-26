#tool dotnet:?package=GitVersion.Tool&version=6.8.2
#tool dotnet:?package=GitReleaseManager.Tool&version=0.20.0
#tool dotnet:?package=dotnet-reportgenerator-globaltool&version=5.6.1
#addin nuget:?package=Spectre.Console&version=0.57.2

using Spectre.Console;

var target        = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solution      = "./Hirondelle.slnx";
var repoName      = "Hirondelle";

GitVersion gitVersion = null;

Setup(ctx =>
{
    // https://gitversion.net/docs/usage/cli/arguments
    // https://cakebuild.net/api/Cake.Core.Tooling/ToolSettings/50AAB3A8
    gitVersion = GitVersion(new GitVersionSettings
    {
        OutputType = GitVersionOutput.Json,
        Verbosity  = GitVersionVerbosity.Verbose,
    });

    AnsiConsole.Write(
        new FigletText(repoName)
            .LeftJustified()
            .Color(Color.Red));

    Information("Configuration             : {0}", configuration);
    Information("Branch                    : {0}", gitVersion.BranchName);
    Information("Informational      Version: {0}", gitVersion.InformationalVersion);
    Information("SemVer             Version: {0}", gitVersion.SemVer);
    Information("AssemblySemVer     Version: {0}", gitVersion.AssemblySemVer);
    Information("AssemblySemFileVer Version: {0}", gitVersion.AssemblySemFileVer);
    Information("MajorMinorPatch    Version: {0}", gitVersion.MajorMinorPatch);
    Information("NuGet              Version: {0}", gitVersion.NuGetVersion);
});

Task("Clean")
    .Does(() => CleanDirectories("./src/**/bin/" + configuration));

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => DotNetRestore(solution));

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration   = configuration,
        NoRestore       = true,
        MSBuildSettings = new DotNetMSBuildSettings()
            .SetVersion(gitVersion.SemVer)
            .SetAssemblyVersion(gitVersion.AssemblySemVer)
            .SetFileVersion(gitVersion.AssemblySemFileVer)
            .SetInformationalVersion(gitVersion.InformationalVersion)
    }));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild       = true
    }));

// Runs the tests with Microsoft Code Coverage, then turns the Cobertura file into an HTML report.
// Open ./coveragereport/index.html once done.
Task("Coverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var resultsDirectory = MakeAbsolute(Directory("./TestResults"));
    var reportDirectory  = MakeAbsolute(Directory("./coveragereport"));
    var coverageFile     = resultsDirectory.CombineWithFilePath("coverage.cobertura.xml");

    CleanDirectories(new[] { resultsDirectory, reportDirectory });

    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration    = configuration,
        NoBuild          = true,
        ArgumentCustomization = args => args
            .Append("--results-directory").AppendQuoted(resultsDirectory.FullPath)
            .Append("--coverage")
            .Append("--coverage-output-format cobertura")
            .Append("--coverage-output").AppendQuoted(coverageFile.GetFilename().FullPath)
    });

    ReportGenerator(coverageFile, reportDirectory, new ReportGeneratorSettings
    {
        // Cobertura: the filtered coverage file read by the CI summary (coveragereport/Cobertura.xml).
        ReportTypes  = new[] { ReportGeneratorReportType.Html, ReportGeneratorReportType.TextSummary, ReportGeneratorReportType.Cobertura },
        // Code emitted by the [GeneratedRegex] source generator is not ours to test.
        ClassFilters = new[] { "-System.Text.RegularExpressions.Generated*" }
    });

    Information(System.IO.File.ReadAllText(reportDirectory.CombineWithFilePath("Summary.txt").FullPath));
    Information("Coverage report: {0}", reportDirectory.CombineWithFilePath("index.html"));
});

// Creates (or updates) the draft pre-release of the milestone named after the next version.
// Run by the CI after each push on master; the final release is published manually on GitHub.
Task("Release-Draft")
    .Does(() =>
{
    var token = EnvironmentVariable("GITHUB_TOKEN")
        ?? throw new CakeException("GITHUB_TOKEN is required to create the release draft.");
    var repository = (EnvironmentVariable("GITHUB_REPOSITORY") ?? "jibedoubleve/hirondelle").Split('/');
    var milestone  = gitVersion.MajorMinorPatch;

    if (!MilestoneExists(token, repository[0], repository[1], milestone))
    {
        Warning("No milestone named '{0}': no release draft is created.", milestone);
        return;
    }

    Information("Draft pre-release {0} for commit {1}", milestone, gitVersion.Sha);

    GitReleaseManagerCreate(token, repository[0], repository[1], new GitReleaseManagerCreateSettings
    {
        Milestone       = milestone,
        Name            = milestone,
        Prerelease      = true,
        TargetCommitish = gitVersion.Sha
    });
});

Task("Default").IsDependentOn("Test");

RunTarget(target);

// A commit that does not belong to a planned version (e.g. tooling) has no milestone:
// in that case the release draft is skipped instead of failing the build.
bool MilestoneExists(string token, string owner, string repository, string title)
{
    using var client = new System.Net.Http.HttpClient();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("hirondelle-build");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var json = client
        .GetStringAsync($"https://api.github.com/repos/{owner}/{repository}/milestones?state=all&per_page=100")
        .GetAwaiter()
        .GetResult();

    using var document = System.Text.Json.JsonDocument.Parse(json);
    return document.RootElement
        .EnumerateArray()
        .Any(m => m.GetProperty("title").GetString() == title);
}
