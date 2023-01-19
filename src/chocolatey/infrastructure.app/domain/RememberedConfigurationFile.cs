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

namespace chocolatey.infrastructure.app.domain
{
    using System;
    using System.ComponentModel;
    using System.IO.Packaging;
    using System.Xml.Serialization;
    using configuration;
    using nuget;

    /// <summary>
    ///   A package file in the snapshot.
    /// </summary>
    [Serializable]
    [XmlType("rememberedConfiguration")]
    public sealed class RememberedConfigurationFile
    {
        [XmlElement(ElementName = "installArguments", IsNullable = true)]
        public string InstallArgumentsEncrypted;

        [XmlElement(ElementName = "packageParameters", IsNullable = true)]
        public string PackageParametersEncrypted;

        [XmlElement(ElementName = "username", IsNullable = true)]
        public string UsernameEncrypted;

        [XmlElement(ElementName = "password", IsNullable = true)]
        public string PasswordEncrypted;

        [XmlElement(ElementName = "certificate", IsNullable = true)]
        public string CertificateEncrypted;

        [XmlElement(ElementName = "certificatePassword", IsNullable = true)]
        public string CertificatePasswordEncrypted;

        [XmlElement(ElementName = "prerelease")]
        public bool Prerelease { get; set; }

        [XmlElement(ElementName = "ignoreDependencies")]
        public bool IgnoreDependencies { get; set; }

        [XmlElement(ElementName = "forceX86")]
        public bool ForceX86 { get; set; }

        [XmlElement(ElementName = "overrideArguments")]
        public bool OverrideArguments { get; set; }

        [XmlElement(ElementName = "applyInstallArgumentsToDependencies")]
        public bool ApplyInstallArgumentsToDependencies { get; set; }

        [XmlElement(ElementName = "applyPackageParametersToDependencies")]
        public bool ApplyPackageParametersToDependencies { get; set; }

        [XmlElement(ElementName = "allowDowngrade")]
        public bool AllowDowngrade { get; set; }

        [XmlElement(ElementName = "failOnStderr")]
        public bool FailOnStderr { get; set; }

        [XmlElement(ElementName = "usePowerShellHost")]
        public bool UsePowerShellHost { get; set; }

        /// <summary>
        ///   Gets or sets the install arguments
        /// </summary>
        /// <value>
        ///   The install arguments
        /// </value>
        [XmlIgnore]
        public string InstallArguments
        {
            get => string.IsNullOrWhiteSpace(InstallArgumentsEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(InstallArgumentsEncrypted);
            set => InstallArgumentsEncrypted = string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the package parameters
        /// </summary>
        /// <value>
        ///   The package parameters
        /// </value>
        [XmlIgnore]
        public string PackageParameters {
            get => string.IsNullOrWhiteSpace(PackageParametersEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(PackageParametersEncrypted);
            set => PackageParametersEncrypted = string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the user
        /// </summary>
        /// <value>
        ///   The user
        /// </value>
        [XmlIgnore]
        public string Username
        {
            get => string.IsNullOrWhiteSpace(UsernameEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(UsernameEncrypted);
            set => UsernameEncrypted = string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the password
        /// </summary>
        /// <value>
        ///   The password
        /// </value>
        [XmlIgnore]
        public string Password
        {
            get => string.IsNullOrWhiteSpace(PasswordEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(PasswordEncrypted);
            set => PasswordEncrypted= string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the certificate
        /// </summary>
        /// <value>
        ///   The certificate
        /// </value>
        [XmlIgnore]
        public string Certificate
        {
            get => string.IsNullOrWhiteSpace(CertificateEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(CertificateEncrypted);
            set => CertificateEncrypted = string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the Certificate Password
        /// </summary>
        /// <value>
        ///   The Certificate Password
        /// </value>
        [XmlIgnore]
        public string CertificatePassword
        {
            get => string.IsNullOrWhiteSpace(CertificatePasswordEncrypted) ? string.Empty : NugetEncryptionUtility.DecryptString(CertificatePasswordEncrypted);
            set => CertificatePasswordEncrypted = string.IsNullOrWhiteSpace(value) ? string.Empty : NugetEncryptionUtility.EncryptString(value);
        }

        /// <summary>
        ///   Gets or sets the Cache Location
        /// </summary>
        /// <value>
        ///   The Cache Location
        /// </value>
        [XmlElement(ElementName = "cacheLocation", IsNullable = true)]
        public string CacheLocation { get; set; }

        [System.ComponentModel.DefaultValue(-1)]
        [XmlElement(ElementName = "commandExecutionTimeoutSeconds", IsNullable = true)]
        public int? CommandExecutionTimeoutSeconds { get; set; }

        [XmlIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool CommandExecutionTimeoutSecondsSpecified
        {
            get { return CommandExecutionTimeoutSeconds != null; }
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="RememberedConfigurationFile" /> class.
        /// </summary>
        public RememberedConfigurationFile(ChocolateyConfiguration config)
        {
            Prerelease = config.Prerelease;
            IgnoreDependencies = config.IgnoreDependencies;
            ForceX86 = config.ForceX86;
            InstallArguments = config.InstallArguments;
            OverrideArguments = config.OverrideArguments;
            ApplyInstallArgumentsToDependencies = config.ApplyInstallArgumentsToDependencies;
            PackageParameters = config.PackageParameters;
            ApplyPackageParametersToDependencies = config.ApplyPackageParametersToDependencies;
            AllowDowngrade = config.AllowDowngrade;
            Username = config.SourceCommand.Username;
            Password = config.SourceCommand.Password;
            Certificate = config.SourceCommand.Certificate;
            CertificatePassword = config.SourceCommand.CertificatePassword;
            if (config.CommandExecutionTimeoutSeconds != ApplicationParameters.DefaultWaitForExitInSeconds)
            {
                CommandExecutionTimeoutSeconds = config.CommandExecutionTimeoutSeconds;
            }
            CacheLocation = config.CacheLocation;
            FailOnStderr = config.Features.FailOnStandardError;
            UsePowerShellHost = config.Features.UsePowerShellHost;

            // https://github.com/chocolatey/choco/blob/80c1c16d3517c1c9baa3d4288e59e320a45ab76f/src/chocolatey/infrastructure.app/services/ChocolateyPackageService.cs#L525-L574
            // Config items that should not be saved:
            // config.Features.ChecksumFiles
            // config.Features.AllowEmptyChecksums
            // config.Features.AllowEmptyChecksumsSecure
            // config.Features.UsePackageExitCodes
            // config.SkipPackageInstallProvider
            // config.UpgradeCommand.FailOnUnfound

            // Config items that we want to save, but would not currently work correctly
            // config.Sources
        }

        public RememberedConfigurationFile()
        {
        }

        public PackagesConfigFilePackageSetting set_packages_config_file_attributes(PackagesConfigFilePackageSetting configElement)
        {
            configElement.Prerelease = Prerelease;
            configElement.IgnoreDependencies = IgnoreDependencies;
            configElement.ForceX86 = ForceX86;
            configElement.InstallArguments = InstallArguments;
            configElement.OverrideArguments = OverrideArguments;
            configElement.ApplyInstallArgumentsToDependencies = ApplyInstallArgumentsToDependencies;
            configElement.PackageParameters = PackageParameters;
            configElement.ApplyPackageParametersToDependencies = ApplyPackageParametersToDependencies;
            configElement.AllowDowngrade = AllowDowngrade;
            configElement.User = Username;
            configElement.Password = Password;
            configElement.Cert = Certificate;
            configElement.CertPassword = CertificatePassword;
            if (CommandExecutionTimeoutSeconds != null) configElement.ExecutionTimeout = (int)CommandExecutionTimeoutSeconds;
            // Don't export cache location, as it commonly can be system specific
            // configElement.CacheLocation = CacheLocation;

            // System Specific
            // configElement.FailOnStderr = FailOnStderr;

            // System Specific
            // configElement.UseSystemPowershell = !UsePowerShellHost;

            return configElement;
        }

        public ChocolateyConfiguration set_remembered_configuration(ChocolateyConfiguration config)
        {
            // Only strings can be overridden by user.
            config.Prerelease = Prerelease;
            config.IgnoreDependencies = IgnoreDependencies;
            config.ForceX86 = ForceX86;
            if (!string.IsNullOrWhiteSpace(config.InstallArguments)) config.InstallArguments = InstallArguments;
            config.OverrideArguments = OverrideArguments;
            config.ApplyInstallArgumentsToDependencies = ApplyInstallArgumentsToDependencies;
            if (!string.IsNullOrWhiteSpace(config.PackageParameters)) config.PackageParameters = PackageParameters;
            config.ApplyPackageParametersToDependencies = ApplyPackageParametersToDependencies;
            config.AllowDowngrade = AllowDowngrade;
            if (!string.IsNullOrWhiteSpace(config.SourceCommand.Username)) config.SourceCommand.Username = Username;
            if (!string.IsNullOrWhiteSpace(config.SourceCommand.Password)) config.SourceCommand.Password = Password;
            if (!string.IsNullOrWhiteSpace(config.SourceCommand.Certificate)) config.SourceCommand.Certificate = Certificate;
            if (!string.IsNullOrWhiteSpace(config.SourceCommand.CertificatePassword)) config.SourceCommand.CertificatePassword = CertificatePassword;
            if (CommandExecutionTimeoutSeconds != null) config.CommandExecutionTimeoutSeconds = (int)CommandExecutionTimeoutSeconds;
            // Cache location default would already be set, so we can't check if user overridden
            config.CacheLocation = CacheLocation;
            config.Features.FailOnStandardError = FailOnStderr;
            config.Features.UsePowerShellHost = UsePowerShellHost;

            return config;
        }
    }
}
