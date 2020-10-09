using System.Threading;
using System.Threading.Tasks;

namespace DockerUpgradeTool
{
    public interface IGitRepositoryProcessor
    {
        Task ProcessAsync(CancellationToken cancellationToken);
    }
}
