using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Git
{
    public interface ILocalGitRepository
    {
        string WorkingDirectory { get; }

        bool IsDirty { get; }
        IDirectoryInfo Directory { get; }

        bool Ignored(IFileInfo file);

        Task CreatePullRequestAsync(IRemoteGitRepository forkedRepository, IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken);
        
        void Reset();
    }
}
