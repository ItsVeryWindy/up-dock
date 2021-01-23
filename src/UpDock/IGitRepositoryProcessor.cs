using System.Threading;
using System.Threading.Tasks;

namespace UpDock
{
    public interface IGitRepositoryProcessor
    {
        Task ProcessAsync(CancellationToken cancellationToken);
    }
}
