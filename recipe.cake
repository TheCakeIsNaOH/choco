#load nuget:?package=Chocolatey.Cake.Recipe&version=0.28.4
#tool nuget:?package=WiX&version=3.11.2

///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// SCRIPT
///////////////////////////////////////////////////////////////////////////////


Func<FilePathCollection> getScriptsToVerify = () =>
{
    var scriptsToVerify = GetFiles(BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/**/*.{ps1|psm1|psd1}") +
                        GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/**/*.{ps1|psm1|psd1}");

    if (DirectoryExists(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip"))
    {
        scriptsToVerify += GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/**/*.{ps1|psm1|psd1}");
    }

    Information("The following PowerShell scripts have been selected to be verified...");
    foreach (var scriptToVerify in scriptsToVerify)
    {
        Information(scriptToVerify.FullPath);
    }

    return scriptsToVerify;
};

Func<FilePathCollection> getScriptsToSign = () =>
{
    var scriptsToSign = GetFiles("./nuspec/**/*.{ps1|psm1|psd1}") +
                        GetFiles("./src/chocolatey.resources/**/*.{ps1|psm1|psd1}");

    Information("The following PowerShell scripts have been selected to be signed...");
    foreach (var scriptToSign in scriptsToSign)
    {
        Information(scriptToSign.FullPath);
    }

    return scriptsToSign;
};

Func<FilePathCollection> getFilesToSign = () =>
{
    var filesToSign = GetFiles(BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/lib/chocolatey.dll")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/choco.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/tools/{checksum|shimgen}.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/redirects/*.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll");

    if (DirectoryExists(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip"))
    {
        filesToSign += GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/tools/chocolateyInstall/choco.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/tools/chocolateyInstall/tools/{checksum|shimgen}.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/tools/chocolateyInstall/redirects/*.exe")
                    + GetFiles(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll");
    }

    Information("The following assemblies have been selected to be signed...");
    foreach (var fileToSign in filesToSign)
    {
        Information(fileToSign.FullPath);
    }

    return filesToSign;
};

Func<FilePathCollection> getMsisToSign = () =>
{
    var msisToSign = GetFiles(BuildParameters.Paths.Directories.Build + "/MSIs/**/*.msi");

    Information("The following msi's have been selected to be signed...");
    foreach (var msiToSign in msisToSign)
    {
        Information(msiToSign.FullPath);
    }

    return msisToSign;
};

///////////////////////////////////////////////////////////////////////////////
// CUSTOM TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Prepare-Chocolatey-Packages")
    .IsDependeeOf("Create-Chocolatey-Packages")
    .IsDependeeOf("Verify-PowerShellScripts")
    .IsDependeeOf("Sign-Assemblies")
    .WithCriteria(() => BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows, "Skipping because not running on Windows")
    .WithCriteria(() => BuildParameters.ShouldRunChocolatey, "Skipping because execution of Chocolatey has been disabled")
    .Does(() =>
{
    // Copy legal documents
    CopyFile(BuildParameters.RootDirectoryPath + "/docs/legal/CREDITS.md", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/CREDITS.txt");

    // Run Chocolatey Unpackself
    //CopyFile(BuildParameters.Paths.Directories.PublishedApplications + "/choco/net8.0/choco.exe", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/choco.exe");
    CopyFiles(BuildParameters.Paths.Directories.PublishedApplications + "/choco/net8.0/**/*", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall", true);

    StartProcess(BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/choco.exe", new ProcessSettings{ Arguments = "unpackself -f -y --allow-unofficial-build --run-actual" });

    // Copy Chocolatey.PowerShell.dll and its help.xml file
    CopyFile(BuildParameters.Paths.Directories.PublishedLibraries + "/Chocolatey.PowerShell/net461/Chocolatey.PowerShell.dll", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll");
    CopyFile(BuildParameters.Paths.Directories.PublishedLibraries + "/Chocolatey.PowerShell/net461/Chocolatey.PowerShell.dll-help.xml", BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll-help.xml");

    // Tidy up logs and config folder which are not required
    var logsDirectory = BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/logs";
    var configDirectory = BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "/tools/chocolateyInstall/config";

    if (DirectoryExists(logsDirectory))
    {
        DeleteDirectory(logsDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }

    if (DirectoryExists(configDirectory))
    {
        DeleteDirectory(configDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
});

Task("Build-ChocolateyNo7zip")
    .WithCriteria(() => BuildParameters.Configuration == "ReleaseOfficial", "Skipping No7zip because this isn't an official release")
    .IsDependentOn("DotNetBuild")
    .IsDependentOn("DotNetTest")
    .Does<BuildData>(data => RequireTool(ToolSettings.MSBuildExtensionPackTool, () =>
{
    Information("Building {0} with No7zip", BuildParameters.SolutionFilePath);

    CleanDirectory(BuildParameters.Paths.Directories.PublishedApplications + "/choco-no7zip/");

    var no7zLogPath = BuildParameters.Paths.Files.BuildLogFilePath.ToString().Replace("\\.(\\S+)$", "-no7zip.${1}");

    if (BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows)
    {
        var msbuildSettings = new MSBuildSettings()
            {
                ToolPath = ToolSettings.MSBuildToolPath
            }
            .SetPlatformTarget(ToolSettings.BuildPlatformTarget)
            .UseToolVersion(ToolSettings.BuildMSBuildToolVersion)
            .WithProperty("OutputPath", MakeAbsolute(new DirectoryPath(BuildParameters.Paths.Directories.PublishedApplications + "/choco-no7zip/")).FullPath)
            .WithProperty("TreatWarningsAsErrors", BuildParameters.TreatWarningsAsErrors.ToString())
            .WithTarget("Build")
            .SetMaxCpuCount(ToolSettings.MaxCpuCount)
            .SetConfiguration("ReleaseOfficialNo7zip")
            .WithLogger(
                Context.Tools.Resolve("MSBuild.ExtensionPack.Loggers.dll").FullPath,
                "XmlFileLogger",
                string.Format(
                    "logfile=\"{0}\";invalidCharReplacement=_;verbosity=Detailed;encoding=UTF-8",
                    no7zLogPath
                )
            );

        MSBuild(BuildParameters.SolutionFilePath, msbuildSettings);
    }

    if (FileExists(no7zLogPath))
    {
        BuildParameters.BuildProvider.UploadArtifact(no7zLogPath);
    }
}));

Task("Prepare-ChocolateyNo7zip-Package")
    .WithCriteria(() => BuildParameters.Configuration == "ReleaseOfficial", "Skipping No7zip because this isn't an official release")
    .WithCriteria(() => BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows, "Skipping because not running on Windows")
    .WithCriteria(() => BuildParameters.ShouldRunChocolatey, "Skipping because execution of Chocolatey has been disabled")
    .IsDependentOn("Build-ChocolateyNo7zip")
    .IsDependeeOf("Sign-Assemblies")
    .IsDependeeOf("Verify-PowerShellScripts")
    .IsDependeeOf("Create-ChocolateyNo7zip-Package")
    .Does(() =>
{
    var nuspecDirectory = BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip";
    // Copy the Nuget/Chocolatey directory from Root Folder to temp/nuspec/chocolatey-no7zip
    EnsureDirectoryExists(nuspecDirectory);
    CopyFiles(GetFiles("./nuspec/chocolatey/**/*"), nuspecDirectory, true);

    // Copy legal documents
    CopyFile(BuildParameters.RootDirectoryPath + "/docs/legal/CREDITS.md", nuspecDirectory + "/tools/chocolateyInstall/CREDITS.txt");

    // Run Chocolatey Unpackself
    CopyFile(BuildParameters.Paths.Directories.PublishedApplications + "/choco/net8.0/choco.exe", nuspecDirectory + "/tools/chocolateyInstall/choco.exe");

    StartProcess(nuspecDirectory + "/tools/chocolateyInstall/choco.exe", new ProcessSettings{ Arguments = "unpackself -f -y --allow-unofficial-build" });

    // Copy Chocolatey.PowerShell.dll and help.xml file
    CopyFile(BuildParameters.Paths.Directories.PublishedLibraries + "/Chocolatey.PowerShell/Chocolatey.PowerShell.dll", nuspecDirectory + "/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll");
    CopyFile(BuildParameters.Paths.Directories.PublishedLibraries + "/Chocolatey.PowerShell/Chocolatey.PowerShell.dll-help.xml", nuspecDirectory + "/tools/chocolateyInstall/helpers/Chocolatey.PowerShell.dll-help.xml");

    // Tidy up logs and config folder which are not required
    var logsDirectory = nuspecDirectory + "/tools/chocolateyInstall/logs";
    var configDirectory = nuspecDirectory + "/tools/chocolateyInstall/config";

    if (DirectoryExists(logsDirectory))
    {
        DeleteDirectory(logsDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }

    if (DirectoryExists(configDirectory))
    {
        DeleteDirectory(configDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
});

Task("Create-ChocolateyNo7zip-Package")
    .WithCriteria(() => BuildParameters.Configuration == "ReleaseOfficial", "Skipping No7zip because this isn't an official release")
    .WithCriteria(() => BuildParameters.ShouldRunChocolatey, "Skipping because execution of Chocolatey has been disabled")
    .WithCriteria(() => BuildParameters.BuildAgentOperatingSystem == PlatformFamily.Windows, "Skipping because not running on Windows")
    .IsDependentOn("Prepare-ChocolateyNo7zip-Package")
    .IsDependeeOf("Package")
    .Does(() =>
{
    var nuspecDirectory = BuildParameters.Paths.Directories.ChocolateyNuspecDirectory + "-no7zip/";
    var nuspecFile = nuspecDirectory + "chocolatey.nuspec";

    ChocolateyPack(nuspecFile, new ChocolateyPackSettings {
        AllowUnofficial = true,
        Version = BuildParameters.Version.PackageVersion,
        OutputDirectory = nuspecDirectory,
        WorkingDirectory = BuildParameters.Paths.Directories.PublishedApplications
    });

    MoveFile(
        nuspecDirectory + "chocolatey." + BuildParameters.Version.PackageVersion + ".nupkg",
        BuildParameters.Paths.Directories.ChocolateyPackages + "/chocolatey-no7zip." + BuildParameters.Version.PackageVersion + ".nupkg"
    );

    // Due to the fact that we have chosen to ignore the no7zip package via the chocolateyNupkgGlobbingPattern, it will
    // no longer be automatically uploaded via Chocolatey.Cake.Recipe, so we need to handle that work here.
    BuildParameters.BuildProvider.UploadArtifact(BuildParameters.Paths.Directories.ChocolateyPackages + "/chocolatey-no7zip." + BuildParameters.Version.PackageVersion + ".nupkg");
});

Task("Prepare-NuGet-Packages")
    .WithCriteria(() => BuildParameters.ShouldRunNuGet, "Skipping because execution of NuGet has been disabled")
    .IsDependeeOf("Create-NuGet-Packages")
    .IsDependeeOf("Verify-PowerShellScripts")
    .IsDependeeOf("Sign-Assemblies")
    .Does(() =>
{
    CleanDirectory(BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/chocolatey.lib/lib");
    EnsureDirectoryExists(BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/chocolatey.lib/lib/net80");

    // Copy legal documents
    CopyFile(BuildParameters.RootDirectoryPath + "/docs/legal/CREDITS.md", BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/chocolatey.lib/lib/CREDITS.txt");

    CopyFiles(BuildParameters.Paths.Directories.PublishedLibraries + "/chocolatey/net8.0/*", BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/chocolatey.lib/lib/net80");
    CopyFile(BuildParameters.Paths.Directories.PublishedLibraries + "/chocolatey/net8.0/chocolatey.xml", BuildParameters.Paths.Directories.NuGetNuspecDirectory + "/chocolatey.lib/lib/net80/chocolatey.xml");
});

Task("Prepare-MSI")
    .WithCriteria(() => BuildParameters.ShouldBuildMsi, "Skipping because creation of MSI has been disabled")
    .WithCriteria(() => BuildParameters.IsTagged, "Skipping because build is not tagged")
    .IsDependeeOf("Build-MSI")
    .Does(() =>
{
    var installScriptPath = BuildParameters.RootDirectoryPath + "/src/chocolatey.install/assets/Install.ps1";

    if (!FileExists(installScriptPath)) 
    {
        DownloadFile(
            "https://community.chocolatey.org/install.ps1",
            installScriptPath
        );
    }
});

BuildParameters.Tasks.BuildMsiTask
    .WithCriteria(() => BuildParameters.IsTagged, "Skipping because build is not tagged");

Task("Create-TarGz-Packages")
    .IsDependentOn("DotNetBuild")
    .IsDependeeOf("Package")
    .WithCriteria(!IsRunningOnWindows(), "Skipping because this is a Windows build")
    .Does(() =>
{
    EnsureDirectoryExists(BuildParameters.Paths.Directories.ChocolateyPackages);

    var outputFile = string.Format(
        "{0}/chocolatey.v{1}.tar.gz",
        MakeAbsolute(new DirectoryPath(BuildParameters.Paths.Directories.ChocolateyPackages.FullPath)),
        BuildParameters.Version.SemVersion
    );

    StartProcess(
        "tar",
        new ProcessSettings {
            Arguments = string.Format(
                "-czvf {0} .",
                outputFile
            ),
            WorkingDirectory = BuildParameters.Paths.Directories.PublishedApplications.FullPath + "/choco/"
        }
    );

    if (FileExists(outputFile))
    {
        BuildParameters.BuildProvider.UploadArtifact(outputFile);
    }
});

///////////////////////////////////////////////////////////////////////////////
// RECIPE SCRIPT
///////////////////////////////////////////////////////////////////////////////

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            solutionFilePath: "./src/chocolatey.sln",
                            solutionDirectoryPath: "./src/chocolatey",
                            resharperSettingsFileName: "chocolatey.sln.DotSettings",
                            title: "Chocolatey",
                            repositoryOwner: "chocolatey",
                            repositoryName: "choco",
                            productName: "Chocolatey",
                            productDescription: "chocolatey is a product of Chocolatey Software, Inc. - All Rights Reserved.",
                            productCopyright: string.Format("Copyright © 2017 - {0} Chocolatey Software, Inc. Copyright © 2011 - 2017, RealDimensions Software, LLC - All Rights Reserved.", DateTime.Now.Year),
                            shouldStrongNameSignDependentAssemblies: false,
                            treatWarningsAsErrors: false,
                            getScriptsToVerify: getScriptsToVerify,
                            getScriptsToSign: getScriptsToSign,
                            getFilesToSign: getFilesToSign,
                            getMsisToSign: getMsisToSign,
                            preferDotNetGlobalToolUsage: !IsRunningOnWindows(),
                            shouldBuildMsi: true,
                            msiUsedWithinNupkg: false,
                            shouldAuthenticodeSignMsis: true,
                            shouldRunNuGet: IsRunningOnWindows(),
                            shouldAuthenticodeSignPowerShellScripts: IsRunningOnWindows(),
                            shouldPublishAwsLambdas: false,
                            shouldRunInspectCode: false,
                            chocolateyNupkgGlobbingPattern: "/**/chocolatey[!-no7zip]*.nupkg");

ToolSettings.SetToolSettings(context: Context);

BuildParameters.PrintParameters(Context);

Build.RunDotNet();
