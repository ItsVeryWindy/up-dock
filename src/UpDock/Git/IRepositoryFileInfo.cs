using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git
{
    public interface IRepositoryFileInfo
    {
        IFileInfo File { get; }
        string RelativePath { get; }
        IDirectoryInfo Root { get; }

        Task StageAsync(CancellationToken cancellationToken);

        Task<bool> IsIgnoredAsync(CancellationToken cancellationToken);
    }
}
