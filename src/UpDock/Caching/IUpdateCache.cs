using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Nodes;

namespace UpDock.Caching
{
    public interface IUpdateCache
    {
        bool HasChanged(IRepository repository, IConfigurationOptions options);
        void Set(IRepository repository, IConfigurationOptions options, IEnumerable<DockerImage?> images);

        Task LoadAsync(CancellationToken cancellationToken);
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
