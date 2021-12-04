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

namespace chocolatey.infrastructure.app.commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using attributes;
    using commandline;
    using configuration;
    using infrastructure.commands;
    using logging;
    using services;

    [CommandFor("uri", "handle URIs")]
    public class ChocolateyUriCommand : ICommand
    {
        private readonly IChocolateyPackageService _packageService;

        public ChocolateyUriCommand(IChocolateyPackageService packageService)
        {
            _packageService = packageService;
        }

        public void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration configuration)
        {
        }

        public virtual void handle_additional_argument_parsing(IList<string> unparsedArguments, ChocolateyConfiguration configuration)
        {
            configuration.Input = string.Join(" ", unparsedArguments);
            var fullUri = unparsedArguments.DefaultIfEmpty(string.Empty).FirstOrDefault(arg => arg.StartsWith("choco://", StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrWhiteSpace(fullUri)) throw new ApplicationException("No URI found in arguments");
            if (fullUri.Length <= 12) throw new ApplicationException("Malformed URI, ensure that protocol version is included");

            var noChocoUri = fullUri.Substring(8);
            var uriProtocolVersion = noChocoUri.Substring(0, 2).to_lower();
            switch (uriProtocolVersion)
            {
                case "v1":
                    this.Log().Debug("URI protocol V1 detected");
                    configuration.PackageNames = noChocoUri.Substring(3);
                    break;
                //TODO, add a better protocol that includes more options, e.g. package parameters, versions, install arguments, --x86, etc
                //Maybe even allow upgrades/uninstalls?
                default:
                    throw new ApplicationException("Invalid URI protocol version.");
            }

        }

        public virtual void handle_validation(ChocolateyConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.PackageNames))
            {
                throw new ApplicationException("No package name found, something went very wrong");
            }
        }

        public virtual void help_message(ChocolateyConfiguration configuration)
        {
            "chocolatey".Log().Info(ChocolateyLoggers.Important, "Uri Command");
            "chocolatey".Log().Info(@"TODO");

            "chocolatey".Log().Info(ChocolateyLoggers.Important, "Usage");
            "chocolatey".Log().Info(@"TODO, waiting until implementation is discussed.");

            "chocolatey".Log().Info(ChocolateyLoggers.Important, "Examples");
            "chocolatey".Log().Info(@"TODO, waiting until implementation is discussed.");

            "chocolatey".Log().Info(ChocolateyLoggers.Important, "Exit Codes");
            "chocolatey".Log().Info(@"
Exit codes that normally result from running this command.

Normal:
 - 0: operation was successful, no issues detected
 - -1 or 1: an error has occurred

If you find other exit codes that we have not yet documented, please
 file a ticket so we can document it at
 https://github.com/chocolatey/choco/issues/new/choose.

");

            "chocolatey".Log().Info(ChocolateyLoggers.Important, "Options and Switches");
        }

        public virtual void noop(ChocolateyConfiguration configuration)
        {
            _packageService.install_noop(configuration);
        }

        public virtual void run(ChocolateyConfiguration configuration)
        {
            _packageService.ensure_source_app_installed(configuration);
            _packageService.install_run(configuration);
        }

        public virtual bool may_require_admin_access()
        {
            return true;
        }
    }
}