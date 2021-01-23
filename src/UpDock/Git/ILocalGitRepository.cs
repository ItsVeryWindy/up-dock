using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git
{
    public interface ILocalGitRepository
    {
        IDirectoryInfo Directory { get; }
        bool IsDirty { get; }
        IEnumerable<IRepositoryFileInfo> Files { get; }

        Task CreatePullRequestAsync(IRemoteGitRepository forkedRepository, IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken);
        
        void Reset();
    }
}
