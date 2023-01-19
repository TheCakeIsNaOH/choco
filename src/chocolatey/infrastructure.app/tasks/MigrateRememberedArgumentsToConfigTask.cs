// Copyright © 2017 - 2021 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.app.tasks
{
    using System;
    using chocolatey.infrastructure.services;
    using chocolatey.infrastructure.tolerance;
    using System.Linq;
    using events;
    using filesystem;
    using infrastructure.events;
    using infrastructure.tasks;
    using logging;
    using services;
    using chocolatey.infrastructure.app.configuration;
    using chocolatey.infrastructure.app.nuget;
    using chocolatey.infrastructure.app.utility;
    using chocolatey.infrastructure.configuration;
    using System.Collections.Generic;
    using System.IO;
    using chocolatey.infrastructure.app.domain;
    using chocolatey.infrastructure.app.commands;
    using commandline;
    using NuGet.Packaging;

    public class MigrateRememberedArgumentsToConfigTask : ITask
    {
        private readonly IFileSystem _fileSystem;
        private readonly IChocolateyPackageInformationService _packageInformationService;
        private readonly IChocolateyPackageService _packageService;
        private IDisposable _subscription;
        private const string ARGS_FILE = ".arguments";
        private const string REMEMBERED_CONFIG_FILE = ".rememberedConfig";

        public MigrateRememberedArgumentsToConfigTask(IFileSystem fileSystem, IChocolateyPackageInformationService packageInformationService, IChocolateyPackageService packageService)
        {
            _fileSystem = fileSystem;
            _packageInformationService = packageInformationService;
            _packageService = packageService;
        }

        public void initialize()
        {
            _subscription = EventManager.subscribe<PreRunMessage>(handle_message, null, null);
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "{0} is now ready and waiting for {1}.".format_with(GetType().Name, typeof(PreRunMessage).Name));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();
        }

        private void handle_message(PreRunMessage message)
        {
            this.Log().Debug(ChocolateyLoggers.Verbose, "[Migrate] Moving from remembered arguments to remembered configuration...");

            var argumentsFiles = _fileSystem.get_files(ApplicationParameters.ChocolateyPackageInfoStoreLocation, ARGS_FILE, SearchOption.AllDirectories).ToList();
            foreach (var argumentsFile in argumentsFiles.or_empty_list_if_null())
            {
                var packageInfoFolder = _fileSystem.get_directory_name(argumentsFile);
                string packageInfoFolderName = _fileSystem.get_directory_info_for(packageInfoFolder).Name;
                var packageArgumentsEncrypted = _fileSystem.read_file(argumentsFile);

                var packageArgumentsUnencrypted = packageArgumentsEncrypted.contains(" --") && packageArgumentsEncrypted.to_string().Length > 4 ? packageArgumentsEncrypted : NugetEncryptionUtility.DecryptString(packageArgumentsEncrypted);

                var sensitiveArgs = true;
                if (!ArgumentsUtility.arguments_contain_sensitive_information(packageArgumentsUnencrypted))
                {
                    sensitiveArgs = false;
                    this.Log().Debug(ChocolateyLoggers.Verbose, "{0} - Adding remembered arguments for migration: {1}".format_with(packageInfoFolderName, packageArgumentsUnencrypted.escape_curly_braces()));
                }

                var packageArgumentsSplit = packageArgumentsUnencrypted.Split(new[] { " --" }, StringSplitOptions.RemoveEmptyEntries);
                var packageArguments = new List<string>();
                foreach (var packageArgument in packageArgumentsSplit.or_empty_list_if_null())
                {
                    var packageArgumentSplit = packageArgument.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var optionName = packageArgumentSplit[0].to_string();
                    var optionValue = string.Empty;
                    if (packageArgumentSplit.Length == 2)
                    {
                        optionValue = packageArgumentSplit[1].to_string().remove_surrounding_quotes();
                        if (optionValue.StartsWith("'")) optionValue.remove_surrounding_quotes();
                    }

                    if (sensitiveArgs)
                    {
                        this.Log().Debug(ChocolateyLoggers.Verbose, "{0} - Adding '{1}' to migration arguments. Values not shown due to detected sensitive arguments".format_with(packageInfoFolderName, optionName.escape_curly_braces()));
                    }
                    packageArguments.Add("--{0}{1}".format_with(optionName, string.IsNullOrWhiteSpace(optionValue) ? string.Empty : "=" + optionValue));
                }

                var rememberedConfig = new RememberedConfigurationFile();

                var migrateOptionSet = new OptionSet();

                migrateOptionSet
                    .Add("pre|prerelease",
                        "Prerelease - Include Prereleases? Defaults to false.",
                        option => rememberedConfig.Prerelease = option != null)
                    .Add("i|ignoredependencies|ignore-dependencies",
                        "IgnoreDependencies - Ignore dependencies when installing package(s). Defaults to false.",
                        option => rememberedConfig.IgnoreDependencies = option != null)
                    .Add("x86|forcex86",
                        "ForceX86 - Force x86 (32bit) installation on 64 bit systems. Defaults to false.",
                        option => rememberedConfig.ForceX86 = option != null)
                    .Add("ia=|installargs=|install-args=|installarguments=|install-arguments=",
                        "InstallArguments - Install Arguments to pass to the native installer in the package. Defaults to unspecified.",
                        option => rememberedConfig.InstallArguments = option.remove_surrounding_quotes())
                    .Add("o|override|overrideargs|overridearguments|override-arguments",
                        "OverrideArguments - Should install arguments be used exclusively without appending to current package passed arguments? Defaults to false.",
                        option => rememberedConfig.OverrideArguments = option != null)
                    .Add("argsglobal|args-global|installargsglobal|install-args-global|applyargstodependencies|apply-args-to-dependencies|apply-install-arguments-to-dependencies",
                        "Apply Install Arguments To Dependencies  - Should install arguments be applied to dependent packages? Defaults to false.",
                        option => rememberedConfig.ApplyInstallArgumentsToDependencies = option != null)
                    .Add("params=|parameters=|pkgparameters=|packageparameters=|package-parameters=",
                        "PackageParameters - Parameters to pass to the package. Defaults to unspecified.",
                        option => rememberedConfig.PackageParameters = option.remove_surrounding_quotes())
                    .Add("paramsglobal|params-global|packageparametersglobal|package-parameters-global|applyparamstodependencies|apply-params-to-dependencies|apply-package-parameters-to-dependencies",
                        "Apply Package Parameters To Dependencies  - Should package parameters be applied to dependent packages? Defaults to false.",
                        option => rememberedConfig.ApplyPackageParametersToDependencies = option != null)
                    .Add("allowdowngrade|allow-downgrade",
                        "AllowDowngrade - Should an attempt at downgrading be allowed? Defaults to false.",
                        option => rememberedConfig.AllowDowngrade = option != null)
                    .Add("u=|user=",
                        "User - used with authenticated feeds. Defaults to empty.",
                        option => rememberedConfig.Username = option.remove_surrounding_quotes())
                    .Add("p=|password=",
                        "Password - the user's password to the source. Defaults to empty.",
                        option => rememberedConfig.Password = option.remove_surrounding_quotes())
                    .Add("cert=",
                        "Client certificate - PFX pathname for an x509 authenticated feeds. Defaults to empty. Available in 0.9.10+.",
                        option => rememberedConfig.Certificate = option.remove_surrounding_quotes())
                    .Add("cp=|certpassword=",
                        "Certificate Password - the client certificate's password to the source. Defaults to empty. Available in 0.9.10+.",
                        option => rememberedConfig.CertificatePassword = option.remove_surrounding_quotes())
                    .Add("timeout=|execution-timeout=",
                        "CommandExecutionTimeout (in seconds) - The time to allow a command to finish before timing out. Overrides the default execution timeout in the configuration of {0} seconds. '0' for infinite starting in 0.10.4.".format_with(rememberedConfig.CommandExecutionTimeoutSeconds.to_string()),
                        option =>
                        {
                            int timeout = 0;
                            var timeoutString = option.remove_surrounding_quotes();
                            int.TryParse(timeoutString, out timeout);
                            if (timeout > 0 || timeoutString.is_equal_to("0"))
                            {
                                rememberedConfig.CommandExecutionTimeoutSeconds = timeout;
                            }
                        })
                    .Add("c=|cache=|cachelocation=|cache-location=",
                        "CacheLocation - Location for download cache, defaults to %TEMP% or value in chocolatey.config file.",
                        option => rememberedConfig.CacheLocation = option.remove_surrounding_quotes())
                    .Add("use-system-powershell",
                        "UseSystemPowerShell - Execute PowerShell using an external process instead of the built-in PowerShell host. Should only be used when internal host is failing. Available in 0.9.10+.",
                        option => rememberedConfig.UsePowerShellHost = option != null);

                // this changes config globally
                migrateOptionSet.Parse(packageArguments);

                var configFile = _fileSystem.combine_paths(packageInfoFolder, REMEMBERED_CONFIG_FILE);
                if (_fileSystem.file_exists(configFile)) _fileSystem.delete_file(configFile);
                _packageInformationService.save_remembered_config_to_file(rememberedConfig, configFile);
            }
            ConfigurationOptions.reset_options();
        }
    }
}
