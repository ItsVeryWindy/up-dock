using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Imaging;
using UpDock.Nodes;

namespace UpDock.Registry
{
    public interface IVersionCache
    {
        Task UpdateCacheAsync(IEnumerable<DockerImageTemplate> templates, CancellationToken cancellationToken);

        DockerImage? FetchLatest(DockerImageTemplate template);
    }
}
