using System.Threading;
using System.Threading.Tasks;

namespace DockerUpgradeTool.CommandLine
{
    public interface ICommandLineRunner<T>
    {
        Task RunAsync(T options, CancellationToken cancellationToken);
    }
}
