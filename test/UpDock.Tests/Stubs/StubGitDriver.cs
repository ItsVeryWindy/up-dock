using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Globbing;
using UpDock.Files;
using UpDock.Git;
using UpDock.Git.Drivers;

namespace UpDock.Tests.Stubs
{
    public class StubGitDriver : IGitDriver
    {
        private Dictionary<string, StubRepository> _repositories = new();

        public async Task<IRepository> CloneAsync(string cloneUrl, IDirectoryInfo directory, string? token, CancellationToken cancellationToken)
        {
            if (!_repositories.TryGetValue(cloneUrl, out var remoteRepository))
                throw new ArgumentException("Remote repository does not exist", nameof(cloneUrl));

            var repository = new StubRepository(directory, remoteRepository);

            var head = repository.Branches[repository.Head];

            foreach(var commit in head.Commits)
            {
                foreach(var file in commit.Files)
                {
                    var fileInfo = directory.GetFile(file.Key);

                    using var stream = fileInfo.CreateWriteStream();

                    using var ms = new MemoryStream(file.Value);

                    await ms.CopyToAsync(stream, cancellationToken);
                }
            }

            return repository;
        }

        public Task CreateRemoteAsync(IDirectoryInfo remoteDirectory, CancellationToken cancellationToken)
        {
            _repositories[remoteDirectory.AbsolutePath] = new StubRepository(remoteDirectory);

            return Task.CompletedTask;
        }

        private class StubRepository : IRepository
        {
            private readonly HashSet<string> _remotes = new();

            public IEnumerable<IRepositoryFileInfo> Files => Directory.AllFiles.Select(x => new RepositoryFileInfo(this, x));
            public IDirectoryInfo Directory { get; }
            public StubRepository? RemoteRepository { get; }
            public Dictionary<string, byte[]> StagedFiles { get; } = new();
            public string Head { get; set; }
            public Dictionary<string, StubBranch> Branches { get; } = new();

            public StubRepository(IDirectoryInfo directory, StubRepository remoteRepository)
            {
                Directory = directory;
                RemoteRepository = remoteRepository;

                foreach(var remoteBranch in remoteRepository.Branches.Values)
                {
                    var branch = new StubBranch(this, remoteBranch.FullName, remoteBranch.Name, remoteBranch.IsRemote);

                    foreach(var commit in remoteBranch.Commits)
                    {
                        branch.Commits.Enqueue(commit);
                    }

                    Branches[remoteBranch.Name] = branch;
                }

                Head = remoteRepository.Head;

                _remotes.Add("origin");
            }

            public StubRepository(IDirectoryInfo directory)
            {
                Directory = directory;

                Branches.Add("master", new StubBranch(this, "refs/heads/master", "master", false));

                Head = "master";
            }

            public Task<IRemote> CreateRemoteAsync(string name, IRemoteGitRepository repository, CancellationToken cancellationToken) => throw new NotImplementedException();

            public Task<IBranch> GetHeadAsync(CancellationToken none) => Task.FromResult<IBranch>(Branches[Head]);

            public Task<bool> IsDirtyAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task<IReadOnlyCollection<IRemote>> GetRemotesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<IRemote>>(_remotes.Select(x => new StubRemote(this, x)).ToList());
            
            public Task CommitAsync(string message, string email, CancellationToken cancellationToken)
            {
                Branches[Head].Commits.Enqueue(new StubCommit(message, email, StagedFiles.ToDictionary(x => x.Key, x => x.Value)));

                return Task.CompletedTask;
            }

            public Task<IReadOnlyCollection<IBranch>> GetBranchesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<IBranch>>(Branches.Values.OrderBy(x => x.FullName).ToList());
            public Task<IBranch> CreateBranchAsync(string name, CancellationToken cancellationToken)
            {
                var branch = new StubBranch(this, $"refs/heads/{name}", name, false);

                Branches[name] = branch;

                return Task.FromResult<IBranch>(branch);
            }

            private class RepositoryFileInfo : IRepositoryFileInfo
            {
                private readonly StubRepository _repository;

                public RepositoryFileInfo(StubRepository repository, IFileInfo file)
                {
                    _repository = repository;
                    File = file;
                }

                public IFileInfo File { get; }

                public async Task<bool> IsIgnoredAsync(CancellationToken cancellationToken)
                {
                    var ignoreFile = _repository.Directory.GetFile(".gitignore");

                    if (!ignoreFile.Exists)
                        return false;

                    using var sr = new StreamReader(ignoreFile.CreateReadStream()!);

                    string? line;

                    while((line = await sr.ReadLineAsync()) is not null)
                    {
                        if (Glob.Parse(line).IsMatch(RelativePath))
                            return true;
                    }

                    return false;
                }

                public string RelativePath => Path.GetRelativePath(_repository.Directory.AbsolutePath, File.AbsolutePath);

                public IDirectoryInfo Root => _repository.Directory;

                public async Task StageAsync(CancellationToken cancellationToken)
                {
                    using var ms = new MemoryStream();

                    await File.CreateReadStream()!.CopyToAsync(ms);

                    ms.Position = 0;

                    _repository.StagedFiles[RelativePath] = ms.ToArray();
                }
            }
        }

        private class StubBranch : IBranch
        {
            private readonly StubRepository _repository;

            public string Name { get; }

            public string FullName { get; }

            public bool IsRemote { get; }

            public Queue<StubCommit> Commits { get; } = new();

            public StubBranch(StubRepository repository, string fullName, string name, bool isRemote)
            {
                _repository = repository;
                FullName = fullName;
                Name = name;
                IsRemote = isRemote;
            }

            public Task CheckoutAsync(bool force, CancellationToken cancellationToken)
            {
                _repository.Head = Name;

                return Task.CompletedTask;
            }

            public Task<IRemoteBranch> TrackAsync(IRemote remote, CancellationToken cancellationToken)
            {
                return Task.FromResult<IRemoteBranch>(new StubRemoteBranch(_repository, this, new StubRemote(_repository, remote.Name)));
            }
        }

        private class StubRemoteBranch : IRemoteBranch
        {
            private readonly StubRepository _repository;
            private readonly StubBranch _branch;
            private readonly StubRemote _remote;

            public string Name => _branch.Name;

            public string FullName => _branch.FullName;

            public bool IsRemote => _branch.IsRemote;

            public StubRemoteBranch(StubRepository repository, StubBranch branch, StubRemote remote)
            {
                _repository = repository;
                _branch = branch;
                _remote = remote;
            }

            public Task CheckoutAsync(bool force, CancellationToken cancellationToken) => _branch.CheckoutAsync(force, cancellationToken);

            public Task<IRemoteBranch> TrackAsync(IRemote remote, CancellationToken cancellationToken) => _branch.TrackAsync(remote, cancellationToken);

            public Task PushAsync(CancellationToken cancellationToken)
            {
                if (!_repository.RemoteRepository!.Branches.TryGetValue(Name, out var remoteBranch))
                {
                    remoteBranch = new StubBranch(_repository.RemoteRepository, FullName, Name, false);

                    _repository.RemoteRepository!.Branches.Add(Name, remoteBranch);
                }

                var name = $"{_remote!.Name}/{Name}";

                if (!_repository.Branches.ContainsKey(name))
                {
                    _repository.Branches.Add(name, new StubBranch(_repository, $"refs/remotes/{name}", name, true));
                }

                while (_branch.Commits.Count > 0)
                {
                    remoteBranch.Commits.Enqueue(_branch.Commits.Dequeue());
                }

                return Task.CompletedTask;
            }
        }

        private class StubCommit
        {
            private string _message;
            private string _email;
            private Dictionary<string, byte[]> _stagedFiles;

            public IReadOnlyDictionary<string, byte[]> Files => _stagedFiles;

            public StubCommit(string message, string email, Dictionary<string, byte[]> stagedFiles)
            {
                _message = message;
                _email = email;
                _stagedFiles = stagedFiles;
            }
        }

        private class StubRemote : IRemote
        {
            private readonly StubRepository _repository;

            public string Name { get; }

            public StubRemote(StubRepository repository, string name)
            {
                _repository = repository;
                Name = name;
            }

            public Task<IEnumerable<IRemoteReference>> GetReferencesAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<IRemoteReference>>(_repository.Branches.Values.Select(x => new StubRemoteReference(_repository, x)));
            }
        }

        private class StubRemoteReference : IRemoteReference
        {
            private StubRepository _repository;
            private StubBranch _branch;

            public StubRemoteReference(StubRepository repository, StubBranch branch)
            {
                _repository = repository;
                _branch = branch;
            }

            public string FullName => _branch.FullName;

            public Task RemoveAsync(CancellationToken cancellationToken)
            {
                _repository.Branches.Remove(_branch.Name);

                return Task.CompletedTask;
            }
        }
    }
}
