using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class GitProcessRepository : IRepository
    {
        private readonly GitProcess _process;
        private readonly IDirectoryInfo _directory;

        public GitProcessRepository(GitProcess process, IDirectoryInfo directory)
        {
            _process = process;
            _directory = directory;
        }

        public async Task<bool> IsDirtyAsync(CancellationToken cancellationToken)
        {
            var result = await _process.ExecuteAsync(cancellationToken, "status");

            await result.EnsureSuccessExitCodeAsync();

            var content = await result.ReadContentAsync();

            return content.Contains("modified:");
        }

        public IEnumerable<IRepositoryFileInfo> Files => Directory.AllFiles.Select(x => new RepositoryFileInfo(this, x));

        public IDirectoryInfo Directory => _directory;

        public async Task<IReadOnlyCollection<IRemote>> GetRemotesAsync(CancellationToken cancellationToken)
        {
            using var result = await _process.ExecuteAsync(cancellationToken, "remote");

            await result.EnsureSuccessExitCodeAsync();

            var remotes = new List<string>();

            await foreach (var t in result.ReadLinesAsync())
            {
                remotes.Add(t);
            }

            return remotes.Select(x => new GitProcessRemote(_process, x)).ToList();
        }

        public async Task<IRemote> CreateRemoteAsync(string name, IRemoteGitRepository repository, CancellationToken cancellationToken)
        {
            var result = await _process.ExecuteAsync(cancellationToken, "remote", "add", name, repository.CloneUrl);

            await result.EnsureSuccessExitCodeAsync();

            return new GitProcessRemote(_process, name);
        }

        private async Task<string> GetHeadReferenceAsync(CancellationToken cancellationToken)
        {
            var result = await _process.ExecuteAsync(cancellationToken, "symbolic-ref", "HEAD");

            await result.EnsureSuccessExitCodeAsync();

            var reference = await result.ReadContentAsync();

            return reference;
        }

        public async Task<IBranch> GetHeadAsync(CancellationToken cancellationToken)
        {
            var reference = await GetHeadReferenceAsync(cancellationToken);

            var result = await _process.ExecuteAsync(cancellationToken, "branch", "--show-current");

            await result.EnsureSuccessExitCodeAsync();

            var name = await result.ReadContentAsync();

            var branch = new GitProcessBranch(_process, this, reference, name, null);

            return branch;
        }

        public async Task CommitAsync(string message, string email, CancellationToken cancellationToken)
        {
            const string name = "up-dock";

            var result = await _process.ExecuteAsync(cancellationToken, "-c", $"user.email={email}", "-c", $"user.name={name}", "-c", $"author.email={email}", "-c", $"author.name={name}", "commit", "-m", message);

            await result.EnsureSuccessExitCodeAsync();
        }

        public async Task<IReadOnlyCollection<IBranch>> GetBranchesAsync(CancellationToken cancellationToken)
        {
            using var result = await _process.ExecuteAsync(cancellationToken, "branch", "-a", "--format", "%(refname)\t%(refname:short)");

            await result.EnsureSuccessExitCodeAsync();

            var branches = new List<IBranch>();

            await foreach (var line in result.ReadLinesAsync())
            {
                var split = line.Split('\t');

                branches.Add(new GitProcessBranch(_process, this, split[0], split[1], null));
            }

            return branches;
        }

        public async Task<IBranch> CreateBranchAsync(string name, CancellationToken cancellationToken)
        {
            var result = await _process.ExecuteAsync(cancellationToken, "branch", name);

            await result.EnsureSuccessExitCodeAsync();

            return new GitProcessBranch(_process, this, name, name, null);
        }

        public void Dispose() { }

        private class RepositoryFileInfo : IRepositoryFileInfo
        {
            private readonly GitProcessRepository _repository;

            public RepositoryFileInfo(GitProcessRepository repository, IFileInfo file)
            {
                _repository = repository;
                File = file;
            }

            public IFileInfo File { get; }

            public async Task<bool> IsIgnoredAsync(CancellationToken cancellationToken)
            {
                var result = await _repository._process.ExecuteAsync(cancellationToken, "check-ignore", RelativePath);

                var exitCode = await result.EnsureExitCodeAsync(0, 1);

                return exitCode == 0;
            }

            public string RelativePath => Path.GetRelativePath(_repository._directory.AbsolutePath, File.AbsolutePath);

            public IDirectoryInfo Root => _repository.Directory;

            public async Task StageAsync(CancellationToken cancellationToken)
            {
                var result = await _repository._process.ExecuteAsync(cancellationToken, "add", RelativePath);

                await result.EnsureSuccessExitCodeAsync();
            }
        }
    }
}
