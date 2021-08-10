using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Nodes;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace UpDock.Git
{
    public class LocalGitRepository : ILocalGitRepository
    {
        private readonly Repository _localRepository;
        private readonly CommandLineOptions _options;
        private readonly IRemoteGitRepository _remoteRepository;
        private readonly IFileProvider _provider;
        private readonly ILogger<LocalGitRepository> _logger;

        public IEnumerable<IRepositoryFileInfo> Files => Directory.Files.Select(x => new LocalGitRepositoryFileInfo(this, x));

        public IDirectoryInfo Directory => _provider.GetDirectory(_localRepository.Info.WorkingDirectory);

        public bool IsDirty => _localRepository.RetrieveStatus(new StatusOptions()).IsDirty;

        public LocalGitRepository(Repository localRepository, CommandLineOptions options, IRemoteGitRepository remoteRepository, IFileProvider provider, ILogger<LocalGitRepository> logger)
        {
            _localRepository = localRepository;
            _options = options;
            _remoteRepository = remoteRepository;
            _provider = provider;
            _logger = logger;
        }

        private bool Ignored(IRepositoryFileInfo file) => _localRepository.Ignore.IsPathIgnored(file.RelativePath);

        public async Task<string?> CreatePullRequestAsync(IRemoteGitRepository forkedRepository, IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            var head = _localRepository.Head;

            try
            {
                var remote = GetRemote(forkedRepository);

                var groupHash = CreateHash(replacements.First().Group);

                var hash = CreateHash(replacements);

                var branch = CreateBranch($"up-dock-{groupHash}-{hash}", remote);

                CleanUpOldReferences(remote, groupHash, branch);

                var newPullRequest = CreatePullRequest(replacements, branch);

                CreateCommit(newPullRequest.Title, branch, remote);

                _logger.LogInformation("Creating pull request {Title} for repository {Repository}", newPullRequest.Title, _remoteRepository.CloneUrl);

                return await _remoteRepository.CreatePullRequestAsync(forkedRepository, newPullRequest);
            }
            finally
            {
                Commands.Checkout(_localRepository, head);
            }
        }

        private Remote GetRemote(IRemoteGitRepository forkedRepository)
        {
            const string remoteName = "downstream";

            var existingRemote = _localRepository.Network.Remotes.FirstOrDefault(x => x.Name == remoteName);

            if (existingRemote != null)
                return existingRemote;

            var newRemote = _localRepository.Network.Remotes.Add(remoteName, forkedRepository.CloneUrl);

            return newRemote;
        }

        private void CleanUpOldReferences(Remote remote, string groupHash, Branch branch)
        {
            var references = _localRepository.Network.ListReferences(remote, CreateCredentials);

            foreach (var reference in references)
            {
                if (!reference.CanonicalName.StartsWith($"refs/heads/up-dock-{groupHash}"))
                    continue;

                if (reference.CanonicalName == branch.CanonicalName)
                    continue;

                _logger.LogInformation("Removing old reference {Reference} for repository {Repository}", reference.CanonicalName, _remoteRepository.CloneUrl);

                _localRepository.Network.Push(remote, $"+:{reference.CanonicalName}", new PushOptions
                {
                    CredentialsProvider = CreateCredentials
                });
            }
        }

        private void CreateCommit(string title, Branch branch, Remote remote)
        {
            var author = new Signature("up-dock", _options.Email, DateTime.Now);

            Commands.Stage(_localRepository, "*");

            _logger.LogInformation("Creating commit {Title}", title);

            _localRepository.Commit(title, author, author);

            var pushRefSpec = string.Format("+{0}:{0}", branch.CanonicalName);

            _logger.LogInformation("Pushing to remote {RefSpec}", pushRefSpec);

            _localRepository.Network.Push(remote, pushRefSpec, new PushOptions
            {
                CredentialsProvider = CreateCredentials
            });
        }

        private PullRequest CreatePullRequest(IReadOnlyCollection<TextReplacement> replacements, Branch branch)
        {
            string title;

            var body = new StringBuilder();

            var distinctReplacements = replacements
                .GroupBy(x => (x.FromPattern.Image, x.ToPattern.Image))
                .ToList();

            if (distinctReplacements.Count == 1)
            {
                var replacement = distinctReplacements.First();

                var image = replacement.First().FromPattern.Image.Template.ToRepositoryImageString();
                var toVersion = replacement.First().ToPattern.Image.Tag;

                title = $"Automatic update of docker image {image} to {toVersion}";

                PopulateSingleUpdate(replacement, body);
            }
            else
            {
                title = $"Automatic update of {distinctReplacements.Count} docker images";

                body
                    .AppendLine($"{distinctReplacements.Count} docker images were updated in {distinctReplacements.SelectMany(x => x).Select(x => x.File.File.AbsolutePath).Distinct().Count()} files:")
                    .AppendLine(string.Join(", ", distinctReplacements.Select(x => $"`{x.Key.Item1.Template.ToRepositoryImageString()}`")))
                    .AppendLine("<details>")
                    .AppendLine("<summary>Details of updated images</summary>");

                foreach (var replacement in distinctReplacements)
                {
                    PopulateSingleUpdate(replacement, body);
                }

                body.AppendLine("</details>");
            }

            body
                .AppendLine()
                .AppendLine("This is an automated update. Merge only if it passes tests.");

            return new PullRequest(
                title,
                branch.FriendlyName,
                _remoteRepository.DefaultBranch, body.ToString());
        }

        private Branch CreateBranch(string name, Remote remote)
        {
            _logger.LogInformation("Creating branch {Name}", name);

            var branch = _localRepository.CreateBranch(name);

            _logger.LogInformation("Tracking branch {Name} with {CanonicalName}", name, branch.CanonicalName);

            _localRepository.Branches.Update(branch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = branch.CanonicalName);

            _logger.LogInformation("Checking out branch {Name}", name);

            Commands.Checkout(_localRepository, branch);

            return branch;
        }

        private static void PopulateSingleUpdate(IGrouping<(DockerImage FromImage, DockerImage ToImage), TextReplacement> replacement, StringBuilder body)
        {
            var image = replacement.First().FromPattern.Image.Template.ToRepositoryImageString();
            var fromVersion = replacement.First().FromPattern.Image.Tag;
            var toVersion = replacement.First().ToPattern.Image.Tag;

            body
                .AppendLine($"UpDock has generated an update of `{image}` from `{fromVersion}` to `{toVersion}`")
                .AppendLine()
                .AppendLine($"{replacement.Count()} file(s) updated");

            foreach (var file in replacement)
            {
                body.AppendLine($"Updated `{file.File.RelativePath}` to `{image}` `{fromVersion}` to `{toVersion}`");
            }
        }

        public void Reset()
        {
            var branch = _localRepository.Branches.First(x => x.IsRemote && x.FriendlyName == $"origin/{_remoteRepository.DefaultBranch}");

            Commands.Checkout(_localRepository, branch, new CheckoutOptions
            {
                CheckoutModifiers = CheckoutModifiers.Force
            });
        }

        private static string CreateHash(IEnumerable<TextReplacement> replacements)
        {
            var createString = replacements
                .Select(x => $"{x.From}{x.To}{x.LineNumber}{x.Start}")
                .Aggregate((x, y) => x + y);

            return CreateHash(createString);
        }

        private static readonly SHA256 Sha256Hash = SHA256.Create();

        private static string CreateHash(string str)
        {
            var bytes = Sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        private UsernamePasswordCredentials CreateCredentials(string url, string user, SupportedCredentialTypes cred)
        {
            return new UsernamePasswordCredentials
            {
                Username = "username", Password = _options.Token
            };
        }

        private class LocalGitRepositoryFileInfo : IRepositoryFileInfo
        {
            private readonly LocalGitRepository _localGitRepository;

            public LocalGitRepositoryFileInfo(LocalGitRepository localGitRepository, IFileInfo file)
            {
                _localGitRepository = localGitRepository;
                File = file;
            }

            public IFileInfo File { get; }

            public bool Ignored => _localGitRepository.Ignored(this);

            public string RelativePath => Path.GetRelativePath(_localGitRepository._localRepository.Info.WorkingDirectory, File.AbsolutePath);

            public IDirectoryInfo Root => _localGitRepository.Directory;
        }
    }
}
