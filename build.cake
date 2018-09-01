///////////////////////////////////////////////////////////////////////////////
// TOOLS AND ADDIN
///////////////////////////////////////////////////////////////////////////////
#tool "nuget:?package=xunit.runner.console"

#tool nuget:?package=gitlink
#addin nuget:?package=Cake.Git

// codecov.io
#tool nuget:?package=Codecov
#addin nuget:?package=Cake.Codecov


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
    CleanDirectories("./build");
    DotNetCoreClean(solution, new DotNetCoreCleanSettings() { Verbosity = DotNetCoreVerbosity.Minimal,});
});

Task("Restore").Does(() =>
{
    DotNetCoreRestore();
    
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
        // vs code problemMatcher workaround
        ArgumentCustomization = args => args.Append("/p:GenerateFullPaths=true"),
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

Task("Coverage").Does(() => {
    var tests = GetFiles("*/*.Tests.csproj");
    foreach(var test in tests)
    {
        Information($"Start tests with coverlet on project {test.GetFilenameWithoutExtension()}");

        var coverageFile = test.GetFilenameWithoutExtension() + ".opencover.xml";
        var coveragePath = Directory("./build/artifacts/coverage/");
        DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings()
        {
            Configuration = "Debug",
            Verbosity = DotNetCoreVerbosity.Minimal,
            NoBuild = false,
            NoRestore = false,
            ResultsDirectory = $"./build/artifacts/tests/",
            // dotnet test has only TRX logger, we add xunit xml logger from XunitXml.TestLogger package
            // https://github.com/xunit/xunit/issues/1154
            // can't use full path because https://github.com/spekt/xunit.testlogger/pull/4
            Logger = $"xunit;LogFilePath=./../build/artifacts/tests/{test.GetFilenameWithoutExtension()}.xml",
            TestAdapterPath = test.GetDirectory(),
            // Don't forget add 
            // <PackageReference Include="coverlet.msbuild" />
            // to csproj with tests
            // Cake.Coverlet has bugs, sometimes throw null reference exception
            // Use plain https://github.com/tonerdo/coverlet
            ArgumentCustomization = args => args.Append("/p:CollectCoverage=true")
                                                .Append("/p:CoverletOutputFormat=opencover")
                                                .AppendSwitchQuoted("/p:CoverletOutput","=", System.IO.Path.Combine(MakeAbsolute(coveragePath).FullPath, coverageFile))
                                                .Append("/p:ExcludeByFile=\"**/AssemblyProperties.cs\""),
            //DiagnosticOutput = false,
            //DiagnosticFile = $"./build/artifacts/tests/{test.GetFilenameWithoutExtension()}_diag.xml",
            
        }); 


        Codecov(System.IO.Path.Combine(coveragePath, coverageFile), "35d84299-29d7-410b-b9c4-9d3c87d54b24");
    }
});

Task("Default").IsDependentOn("Rebuild");

RunTarget(target);