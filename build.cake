#load "build/helpers.cake"
// #addin nuget:?package=Cake.Docker

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
// var framework = Argument("framework", "netcoreapp3.1");

///////////////////////////////////////////////////////////////////////////////
// VERSIONING
///////////////////////////////////////////////////////////////////////////////

var packageVersion = string.Empty;
#load "build/version.cake"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/NexusMiddleware.sln");
var solution = ParseSolution(solutionPath);
var projects = GetProjects(solutionPath, configuration);
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
    packageVersion = BuildVersion(fallbackVersion);
	if (FileExists("./build/.dotnet/dotnet.exe")) {
		Information("Using local install of `dotnet` SDK!");
		Context.Tools.RegisterFile("./build/.dotnet/dotnet.exe");
	}
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projects.AllProjectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	foreach (var project in projects.AllProjectPaths) {
		DotNetCoreRestore(project.FullPath);
	}
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	var settings = new DotNetCoreBuildSettings {
		Configuration = configuration,
		NoIncremental = true,
		ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}")
	};
	DotNetCoreBuild(solutionPath, settings);
});

Task("Run-Unit-Tests")
	.IsDependentOn("Build")
	.Does(() =>
{
    CreateDirectory(testResultsPath);
	if (projects.TestProjects.Any()) {

		var settings = new DotNetCoreTestSettings {
			Configuration = configuration
		};

		foreach(var project in projects.TestProjects) {
			DotNetCoreTest(project.Path.FullPath, settings);
		}
	}
});

Task("Publish-Runtimes")
	.IsDependentOn("Build")
	.Does(() =>
{
	var projectDir = $"{artifacts}publish";
	CreateDirectory(projectDir);
    /* DotNetCorePublish("./src/NexusUploader/NexusUploader.csproj", new DotNetCorePublishSettings {
        OutputDirectory = projectDir + "/dotnet-any",
		Configuration = configuration,
        PublishSingleFile = false,
        PublishTrimmed = false
    }); */
    var runtimes = new[] { "linux-x64", "win-x64"};
    foreach (var runtime in runtimes) {
		var runtimeDir = $"{projectDir}/{runtime}";
		CreateDirectory(runtimeDir);
		Information("Publishing for {0} runtime", runtime);
		var settings = new DotNetCorePublishSettings {
			Runtime = runtime,
			Configuration = configuration,
			OutputDirectory = runtimeDir,
			PublishSingleFile = true,
			PublishTrimmed = true,
			ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}").Append("/p:PackAsTool=false")
		};
		DotNetCorePublish("./src/NexusUploader/NexusUploader.csproj", settings);
		CreateDirectory($"{artifacts}archive");
		Zip(runtimeDir, $"{artifacts}archive/unex-{runtime}.zip");
    }
});

Task("Build-NuGet-Package")
	.IsDependentOn("Publish-Runtimes")
	.Does(() => 
{
	Information("Building dotnet package");
	foreach(var project in projects.SourceProjects) {
		CreateDirectory($"{artifacts}packages/dotnet-any");
		var packSettings = new DotNetCorePackSettings {
			ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}").Append("/p:NoWarn=\"NU5105\""),
			Configuration = configuration,
			OutputDirectory = $"{artifacts}packages/dotnet-any/"
		};
		DotNetCorePack(project.Path.FullPath, packSettings);
	}
});

Task("Publish-NuGet-Package")
.IsDependentOn("Build-NuGet-Package")
.WithCriteria(() => HasEnvironmentVariable("NUGET_TOKEN"))
.WithCriteria(() => EnvironmentVariable("GITHUB_REF").StartsWith("refs/tags/v"))
.Does(() => {
    var nupkgDir = $"{artifacts}packages";
    var nugetToken = EnvironmentVariable("NUGET_TOKEN");
    var pkgFiles = GetFiles($"{nupkgDir}/dotnet-any/*.nupkg");
    NuGetPush(pkgFiles, new NuGetPushSettings {
      Source = "https://api.nuget.org/v3/index.json",
      ApiKey = nugetToken
    });
});

Task("Default")
    .IsDependentOn("Build");

Task("Release")
    .IsDependentOn("Package")
    .IsDependentOn("Publish-NuGet-Package");

Task("Package")
    .IsDependentOn("Publish-Runtimes")
	.IsDependentOn("Build-NuGet-Package");

RunTarget(target);