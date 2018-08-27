///////////////////////////////////////////////////////////////////////////////
// TOOLS AND ADDIN
///////////////////////////////////////////////////////////////////////////////
#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=gitlink"

#addin nuget:?package=Cake.Git

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARS
///////////////////////////////////////////////////////////////////////////////
var solution = File("./ResourceCurator.sln");
var lastCommit = GitLogTip("./");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
    Environment.SetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en");
    Information(@"Last commit {0}
    Short message: {1}
    Author:        {2}
    Authored:      {3:yyyy-MM-dd HH:mm:ss}
    Committer:     {4}
    Committed:     {5:yyyy-MM-dd HH:mm:ss}",
    lastCommit.Sha,
    lastCommit.MessageShort,
    lastCommit.Author.Name,
    lastCommit.Author.When,
    lastCommit.Committer.Name,
    lastCommit.Committer.When
    );
    Information("Running tasks...");
});

Teardown(ctx =>
{
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean").Does(() =>
{
    CleanDirectories(string.Format("./build", configuration));
    DotNetCoreClean(solution, new DotNetCoreCleanSettings()
    {
        Verbosity = DotNetCoreVerbosity.Minimal,
    });
});

Task("Restore").Does(() =>
{
    DotNetCoreRestore(new DotNetCoreRestoreSettings()
    {
        Verbosity = DotNetCoreVerbosity.Minimal,
    });
    
});

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

Task("Build").Does(() => {
    DotNetCoreBuild(solution,  new DotNetCoreBuildSettings() 
    { 
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal,
    });


});
Task("RunTests").Does(() => {
    var tests = GetFiles("*/*.Tests.csproj");
    foreach(var test in tests)
    {
        Information($"Start tests on project {test.GetFilenameWithoutExtension()}");

        
        DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings()
        {
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Minimal,
            NoBuild = true,
            NoRestore = true,
            ResultsDirectory = $"./build/artifacts/tests/",
            // dotnet test has only TRX logger, we add xunit xml logger from XunitXml.TestLogger package
            // https://github.com/xunit/xunit/issues/1154
            // can't use full path because https://github.com/spekt/xunit.testlogger/pull/4
            Logger = $"xunit;LogFilePath=./../build/artifacts/tests/{test.GetFilenameWithoutExtension()}.xml",
            TestAdapterPath = test.GetDirectory(),
            //DiagnosticOutput = false,
            //DiagnosticFile = $"./build/artifacts/tests/{test.GetFilenameWithoutExtension()}_diag.xml",
        });
    }

});

Task("Default").IsDependentOn("Rebuild");

RunTarget(target);