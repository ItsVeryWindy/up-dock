using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgrader.Imaging;

namespace DockerUpgrader.Registry
{
    public interface IVersionCache
    {
        Task UpdateCacheAsync(IEnumerable<DockerImageTemplatePattern> patterns, CancellationToken cancellationToken);

        DockerImagePattern FetchLatest(DockerImagePattern pattern);
    }
}