using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.Files;
using DockerUpgradeTool.Nodes;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Octokit;
using Branch = LibGit2Sharp.Branch;
using Signature = LibGit2Sharp.Signature;

namespace DockerUpgradeTool.Git
{
    public class LocalGitRepository : ILocalGitRepository
    {
        private readonly LibGit2Sharp.Repository _localRepository;
        private readonly CommandLineOptions _options;
        private readonly IGitHubClient _client;
        private readonly IRemoteGitRepository _remoteRepository;
        private readonly ILogger<LocalGitRepository> _logger;

        public string WorkingDirectory => _localRepository.Info.WorkingDirectory;

        public IDirectoryInfo Directory { get; }

        public bool IsDirty => _localRepository.RetrieveStatus(new StatusOptions()).IsDirty;

        public LocalGitRepository(LibGit2Sharp.Repository localRepository, IDirectoryInfo directory, CommandLineOptions options, IGitHubClient client, IRemoteGitRepository remoteRepository, ILogger<LocalGitRepository> logger)
        {
            _localRepository = localRepository;
            _options = options;
            _client = client;
            _remoteRepository = remoteRepository;
            _logger = logger;
            Directory = directory;
        }

        public bool Ignored(IFileInfo file) => _localRepository.Ignore.IsPathIgnored(file.MakeRelativePath(Directory));

        public async Task CreatePullRequestAsync(IRemoteGitRepository forkedRepository, IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            var remote = _localRepository.Network.Remotes.Add("downstream", forkedRepository.CloneUrl);

            var hash = CreateHash(replacements);

            var branch = CreateBranch($"docker-update-{hash}", remote);

            var newPullRequest = CreatePullRequest(forkedRepository, replacements, branch);

            CreateCommit(newPullRequest.Title, branch, remote);

            _logger.LogInformation("Creating pull request {Title} for repository {Repository}", newPullRequest.Title, _remoteRepository.CloneUrl);

            var pullRequest = await  _client.PullRequest.Create(_remoteRepository.Owner, _remoteRepository.Name, newPullRequest);

            var labelUpdate = new IssueUpdate();

            labelUpdate.Labels.Add("docker-upgrade-tool");

            _logger.LogInformation("Updating pull request {Url} with label", pullRequest.Url);

            await _client.Issue.Update(_remoteRepository.Owner, _remoteRepository.Name, pullRequest.Number, labelUpdate);
        }

        private void CreateCommit(string title, Branch branch, Remote remote)
        {
            var author = new Signature("docker-upgrade-tool", _options.Email, DateTime.Now);

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

        private NewPullRequest CreatePullRequest(IRemoteGitRepository forkedRepository, IReadOnlyCollection<TextReplacement> replacements, Branch branch)
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
                    .AppendLine($"{distinctReplacements.Count} docker images were updated in {distinctReplacements.SelectMany(x => x).Select(x => x.File.Path).Distinct().Count()} files:")
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
                .AppendLine("This is an automated update. Merge only if it passes tests");

            return new NewPullRequest(
                title,
                $"{forkedRepository.Owner}:{branch.FriendlyName}",
                _remoteRepository.Branch)
            {
                Body = body.ToString()
            };
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

        private void PopulateSingleUpdate(IGrouping<(DockerImage FromImage, DockerImage ToImage), TextReplacement> replacement, StringBuilder body)
        {
            var image = replacement.First().FromPattern.Image.Template.ToRepositoryImageString();
            var fromVersion = replacement.First().FromPattern.Image.Tag;
            var toVersion = replacement.First().ToPattern.Image.Tag;

            body
                .AppendLine($"Docker Upgrade Tool has generated an update of `{image}` from `{fromVersion}` to `{toVersion}`")
                .AppendLine()
                .AppendLine($"{replacement.Count()} file(s) updated");

            foreach (var file in replacement)
            {
                body.AppendLine($"Updated `{file.File.MakeRelativePath(Directory)}` to `{image}` `{fromVersion}` to `{toVersion}`");
            }
        }

        public void Reset()
        {
            var branch = _localRepository.Branches.First(x => x.IsRemote && x.FriendlyName == $"origin/{_remoteRepository.Branch}");

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

            using var sha256Hash = SHA256.Create();

            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(createString));  
  
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
    }
}
