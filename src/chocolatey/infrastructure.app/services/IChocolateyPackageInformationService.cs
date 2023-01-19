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

namespace chocolatey.infrastructure.app.services
{
    using domain;
    using NuGet.Packaging;
    using System;

    public interface IChocolateyPackageInformationService
    {
        ChocolateyPackageInformation Get(IPackageMetadata package);
        void Save(ChocolateyPackageInformation packageInformation);
        void Remove(IPackageMetadata package);
        
        /// <summary>
        /// Read the remembered configuration file from the specified filepath.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns>RememberedConfigurationFile with entries based on the file if it exists, otherwise null</returns>
        RememberedConfigurationFile read_remembered_config_from_file(string filePath);


        /// <summary>
        /// Saves the config to the specified file path.
        /// </summary>
        /// <param name="snapshot">The configuration snapshot.</param>
        /// <param name="filePath">The file path.</param>
        void save_remembered_config_to_file(RememberedConfigurationFile snapshot, string filePath);

#pragma warning disable IDE1006
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        ChocolateyPackageInformation get_package_information(IPackageMetadata package);
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        void save_package_information(ChocolateyPackageInformation packageInformation);
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        void remove_package_information(IPackageMetadata package);
#pragma warning restore IDE1006
    }
}
