using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chocolatey.infrastructure.app.nuget
{
    using NuGet.Configuration;

    public class ChocolateyPackageSourceProvider : IPackageSourceProvider
    {
        public string ActivePackageSourceName => throw new NotImplementedException();

        public string DefaultPushSource => throw new NotImplementedException();

        public event EventHandler PackageSourcesChanged;

        public void AddPackageSource(PackageSource source)
        {
            throw new NotImplementedException();
        }

        public void DisablePackageSource(string name)
        {
            throw new NotImplementedException();
        }

        public void EnablePackageSource(string name)
        {
            throw new NotImplementedException();
        }

        public PackageSource GetPackageSourceByName(string name)
        {
            throw new NotImplementedException();
        }

        public PackageSource GetPackageSourceBySource(string source)
        {
            throw new NotImplementedException();
        }

        public bool IsPackageSourceEnabled(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PackageSource> LoadPackageSources()
        {
            throw new NotImplementedException();
        }

        public void RemovePackageSource(string name)
        {
            throw new NotImplementedException();
        }

        public void SaveActivePackageSource(PackageSource source)
        {
            throw new NotImplementedException();
        }

        public void SavePackageSources(IEnumerable<PackageSource> sources)
        {
            throw new NotImplementedException();
        }

        public void UpdatePackageSource(PackageSource source, bool updateCredentials, bool updateEnabled)
        {
            throw new NotImplementedException();
        }
    }
}
