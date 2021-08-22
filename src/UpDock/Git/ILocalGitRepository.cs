using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git
{
    public interface ILocalGitRepository : IDisposable
    {
        IDirectoryInfo Directory { get; }
        
        IEnumerable<IRepositoryFileInfo> Files { get; }

        Task<bool> IsDirtyAsync(CancellationToken cancellationToken);

        Task<(string url, string title)?> CreatePullRequestAsync(IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken);
        
        Task ResetAsync(CancellationToken cancellationToken);
    }
}
