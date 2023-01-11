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

namespace chocolatey.tests.infrastructure.app.nuget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using chocolatey.infrastructure.app.configuration;
    using chocolatey.infrastructure.app.domain;
    using chocolatey.infrastructure.app.nuget;
    using chocolatey.infrastructure.filesystem;
    using Moq;
    using NuGet.Common;
    using NuGet.Packaging;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using RichardSzalay.MockHttp;
    using Should;

    public class NugetListSpecs
    {
        private class when_executing_package_search : TinySpec
        {
            private Action because;
            private readonly Mock<ILogger> nugetLogger = new Mock<ILogger>();
            private readonly Mock<IFileSystem> filesystem = new Mock<IFileSystem>();
            private MockHttpMessageHandler handler;
            private ChocolateyConfiguration configuration;
            private IEnumerable<IPackageSearchMetadata> searchResults;

            private IEnumerable<NuGetSourceResources> repoResources;

            public override void Context()
            {
                configuration = new ChocolateyConfiguration();
                nugetLogger.ResetCalls();
                filesystem.ResetCalls();

                configuration.Sources = "https://127.0.0.1:9999/repository/choco/";
                configuration.CacheLocation = "C:\\Windows\\SystemTemp";

                filesystem.Setup(f => f.get_full_path(It.IsAny<string>())).Returns((string a) =>
                {
                    return "C:\\packages\\" + a;
                });

                handler = new MockHttpMessageHandler();
                handler.AutoFlush = true;
            }

            public override void Because()
            {
                because = () =>
                {
                    var repositories = NugetCommon.GetRemoteRepositories(configuration, nugetLogger.Object, filesystem.Object);
                    repoResources = NugetCommon.GetRepositoryResources(repositories);

                    foreach (var sourceRepository in repositories)
                    {
                        var sourceResource = sourceRepository.GetResource<HttpSourceResource>();

                        var newsource = new HttpSource(sourceRepository.PackageSource, () =>
                            Task.FromResult<HttpHandlerResource>(new MockHttpHandler(handler)), NullThrottle.Instance);
                        sourceResource.OverrideHttpSource(newsource);
                    }
                    searchResults = NugetList.ExecutePackageSearch(repoResources, configuration, nugetLogger.Object, isCount: false).GetAwaiter().GetResult().packages;
                };
            }

            [Fact]
            public void should_do_stuff()
            {
                Context();

                var response = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<feed xml:base=""http://community.chocolatey.org/api/v2/"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">
  <title type=""text"">Packages</title>
  <id>http://community.chocolatey.org/api/v2/Packages</id>
  <updated>2023-01-11T23:08:40Z</updated>
  <author>
    <name />
  </author>
  <link rel=""self"" title=""Packages"" href=""Packages"" />
</feed>";

                handler.When("https://127.0.0.1:9999/repository/choco/*").Respond("application/atom+xml", response);

                because();

                searchResults.Count().ShouldEqual(0);
            }
        }

        public class MockHttpHandler : HttpHandlerResource
        {
            public MockHttpHandler(HttpMessageHandler messageHandler)
            {
                MessageHandler = messageHandler;
                ClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
                };
            }

            public override HttpClientHandler ClientHandler { get; }
            public override HttpMessageHandler MessageHandler { get; }
        }
    }
}
