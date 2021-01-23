using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Imaging;

namespace UpDock.Registry
{
    public interface IVersionCache
    {
        Task UpdateCacheAsync(IEnumerable<DockerImageTemplatePattern> patterns, CancellationToken cancellationToken);

        DockerImagePattern? FetchLatest(DockerImagePattern pattern);
    }
}
