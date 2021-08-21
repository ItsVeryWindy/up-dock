using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public interface IGitDriver
    {
        Task<IRepository> CloneAsync(string cloneUrl, IDirectoryInfo directory, string? token, CancellationToken cancellationToken);
        Task CreateRemoteAsync(IDirectoryInfo remoteDirectory, CancellationToken cancellationToken);
    }
}
