using System.Threading;
using System.Threading.Tasks;

namespace UpDock.CommandLine
{
    public interface ICommandLineRunner<T>
    {
        Task RunAsync(T options, CancellationToken cancellationToken);
    }
}
