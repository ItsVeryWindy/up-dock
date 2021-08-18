﻿using System;
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
using Microsoft.Extensions.Logging;
using UpDock.Git.Drivers;

namespace UpDock.Git
{
    public class LocalGitRepository : ILocalGitRepository
    {
        private readonly IRepository _localRepository;
        private readonly CommandLineOptions _options;
        private readonly IRemoteGitRepository _remoteRepository;
        private IRemoteGitRepository? _forkedRepository;
        private readonly IFileProvider _provider;
        private readonly ILogger<LocalGitRepository> _logger;

        public IEnumerable<IRepositoryFileInfo> Files => _localRepository.Files;

        public IDirectoryInfo Directory => _localRepository.Directory;

        public bool IsDirty => _localRepository.IsDirty;

        public LocalGitRepository(IRepository localRepository, CommandLineOptions options, IRemoteGitRepository remoteRepository, IFileProvider provider, ILogger<LocalGitRepository> logger)
        {
            _localRepository = localRepository;
            _options = options;
            _remoteRepository = remoteRepository;
            _provider = provider;
            _logger = logger;
        }

        public async Task<(string url, string title)?> CreatePullRequestAsync(IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            var head = _localRepository.Head;

            try
            {
                var groupHash = CreateHash(replacements.First().Group);

                var hash = CreateHash(replacements);

                var branch = CreateBranch($"up-dock-{groupHash}-{hash}");

                var newPullRequest = CreatePullRequest(replacements, branch);

                CreateCommit(newPullRequest.Title, replacements);

                var remoteRepository = await PushAsync(groupHash, branch);

                if (remoteRepository is null)
                    return null;

                _logger.LogInformation("Creating pull request {Title} for repository {Repository}", newPullRequest.Title, _remoteRepository.CloneUrl);

                return await _remoteRepository.CreatePullRequestAsync(remoteRepository, newPullRequest);
            }
            finally
            {
                head.Checkout();
            }
        }

        private async Task<IRemoteGitRepository?> PushAsync(string groupHash, IBranch branch)
        {
            if (!_options.ForkOnly && _forkedRepository is null && Push(_remoteRepository, "origin", groupHash, branch))
                return _remoteRepository;

            _forkedRepository ??= await _remoteRepository.ForkRepositoryAsync();

            return Push(_forkedRepository, "upstream", groupHash, branch) ? _forkedRepository : null;
        }

        private bool Push(IRemoteGitRepository remoteRepository, string remoteName, string groupHash, IBranch branch)
        {
            var remote = GetRemote(remoteRepository, remoteName);

            var trackedBranch = TrackBranch(branch, remote);

            CleanUpOldReferences(remote, groupHash, branch);

            if (PushCommit(trackedBranch))
                return true;

            _logger.LogError("Failed to push commit to repository {Repository}", remoteRepository.CloneUrl);

            return false;
        }

        private IRemote GetRemote(IRemoteGitRepository repository, string remoteName)
        {
            var existingRemote = _localRepository.Remotes.FirstOrDefault(x => x.Name == remoteName);

            if (existingRemote != null)
                return existingRemote;

            var newRemote = _localRepository.CreateRemote(remoteName, repository);

            return newRemote;
        }

        private void CleanUpOldReferences(IRemote remote, string groupHash, IBranch branch)
        {
            foreach (var reference in remote.Branches)
            {
                if (!reference.FullName.StartsWith($"refs/heads/up-dock-{groupHash}"))
                    continue;

                if (reference.FullName == branch.FullName)
                    continue;

                _logger.LogInformation("Removing old reference {Reference} for repository {Repository}", reference.FullName, _remoteRepository.CloneUrl);

                try
                {
                    reference.Remove();
                }
                catch(PushException ex)
                {
                    _logger.LogError("Push failed for old reference {Reference} with message '{Message}'", ex.Reference, ex.Message);
                }
            }
        }

        private void CreateCommit(string title, IReadOnlyCollection<TextReplacement> replacements)
        {
            var files = replacements
                .Select(x => x.File)
                .ToList();

            foreach (var file in files)
            {
                _logger.LogInformation("Staging file '{File}'", file);

                file.Stage();
            }

            _logger.LogInformation("Creating commit {Title}", title);

            _localRepository.Commit(title, _options.Email!);
        }

        private bool PushCommit(IRemoteBranch branch)
        {
            _logger.LogInformation("Pushing to remote {RefSpec}", branch.FullName);

            try
            {
                branch.Push();
            }
            catch(PushException ex)
            {
                _logger.LogError("Push failed for {RefSpec} with message '{Message}'", ex.Reference, ex.Message);
                return false;
            }

            return true;
        }

        private PullRequest CreatePullRequest(IReadOnlyCollection<TextReplacement> replacements, IBranch branch)
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
                    .AppendLine(string.Join(", ", distinctReplacements.Select(x => $"`{x.Key.Item1.Template.ToRepositoryImageString()}:{x.Key.Item1.Tag}`")))
                    .AppendLine("<details>")
                    .AppendLine("<summary>Details of updated images</summary>");

                var hr = false;

                foreach (var replacement in distinctReplacements)
                {
                    if (hr)
                    {
                        body.AppendLine("<hr>");
                    }

                    PopulateSingleUpdate(replacement, body);

                    hr = true;
                }

                body.AppendLine("</details>");
            }

            body
                .AppendLine()
                .AppendLine("This is an automated update. Merge only if it passes tests.");

            return new PullRequest(
                title,
                branch.Name,
                body.ToString());
        }

        private IBranch CreateBranch(string name)
        {
            _logger.LogInformation("Creating branch {Name}", name);

            var branch = _localRepository.CreateBranch(name);

            _logger.LogInformation("Checking out branch {Name}", name);

            branch.Checkout();

            return branch;
        }

        private IRemoteBranch TrackBranch(IBranch branch, IRemote remote)
        {
            _logger.LogInformation("Tracking branch {Name} with {CanonicalName}", branch.Name, branch.FullName);

            return branch.Track(remote);
        }

        private static void PopulateSingleUpdate(IGrouping<(DockerImage FromImage, DockerImage ToImage), TextReplacement> replacement, StringBuilder body)
        {
            var image = replacement.First().FromPattern.Image.Template.ToRepositoryImageString();
            var fromVersion = replacement.First().FromPattern.Image.Tag;
            var toVersion = replacement.First().ToPattern.Image.Tag;

            body
                .AppendLine()
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
            var branch = _localRepository.Branches.First(x => x.IsRemote && x.Name == $"origin/{_remoteRepository.DefaultBranch}");

            branch.Checkout(true);
        }

        private static string CreateHash(IEnumerable<TextReplacement> replacements)
        {
            var createString = replacements
                .Select(x => $"{x.From}{x.To}{x.LineNumber}{x.Start}")
                .Aggregate((x, y) => x + y);

            return CreateHash(createString);
        }

        private static readonly SHA256 Sha256Hash = SHA256.Create();

        private static string CreateHash(string str) => Sha256Hash.ComputeHash(str);
    }
}
