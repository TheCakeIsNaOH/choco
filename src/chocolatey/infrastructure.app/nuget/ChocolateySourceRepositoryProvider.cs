using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chocolatey.infrastructure.app.nuget
{
    using configuration;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;

    public class ChocolateySourceRepositoryProvider : ISourceRepositoryProvider
    {
        private IEnumerable<SourceRepository> _sourceRepositories;

        public ChocolateySourceRepositoryProvider(ChocolateyConfiguration configuration, ILogger logger)
        {
            _sourceRepositories = NugetCommon.GetRemoteRepositories(configuration, logger);
        }

        public ChocolateySourceRepositoryProvider(IEnumerable<SourceRepository> sourceRepositories)
        {
            _sourceRepositories = sourceRepositories;
        }

        public IPackageSourceProvider PackageSourceProvider => throw new NotImplementedException();

        public SourceRepository CreateRepository(PackageSource source)
        {
            var repository = _sourceRepositories.Where(s => source == s.PackageSource).FirstOrDefault();
            if (repository == null)
            {
                return Repository.Factory.GetCoreV3(source);
            }
            else
            {
                return repository;
            }
        }

        public SourceRepository CreateRepository(PackageSource source, FeedType type)
        {
            var repository = _sourceRepositories.Where(s => source == s.PackageSource).FirstOrDefault();
            if (repository == null)
            {
                return Repository.Factory.GetCoreV3(source);
            }
            else
            {
                return repository;
            }
        }

        public IEnumerable<SourceRepository> GetRepositories()
        {
            return _sourceRepositories;
        }
    }
}
