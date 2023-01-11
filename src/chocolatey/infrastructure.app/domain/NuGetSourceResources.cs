// Copyright © 2017 - 2023 Chocolatey Software, Inc
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
    using NuGet.Protocol.Core.Types;

    public class NuGetSourceResources
    {
        public NuGetSourceResources(SourceRepository repo)
        {
            Repository = repo;
            SearchResource = repo.GetResource<PackageSearchResource>();
            FindPackageByIdResource = repo.GetResource<FindPackageByIdResource>();
            PackageMetadataResource = repo.GetResource<PackageMetadataResource>();
            ListResource = repo.GetResource<ListResource>();
        }

        public SourceRepository Repository;
        public PackageSearchResource SearchResource;
        public FindPackageByIdResource FindPackageByIdResource;
        public PackageMetadataResource PackageMetadataResource;
        public ListResource ListResource;
    }
}