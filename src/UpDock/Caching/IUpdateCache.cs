using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;
using UpDock.Nodes;

namespace UpDock.Caching
{
    public interface IUpdateCache
    {
        bool HasChanged(IRemoteGitRepository repository, IConfigurationOptions options);
        void Set(IRemoteGitRepository repository, IConfigurationOptions options, IEnumerable<DockerImage?> images);

        Task LoadAsync(CancellationToken cancellationToken);
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
