﻿using System.IO;
using System.Threading.Tasks;
using DockerUpgrader.Files;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Octokit;
using Repository = Octokit.Repository;

namespace DockerUpgrader.Git
{
    public class RemoteGitRepository : IRemoteGitRepository
    {
        private readonly Repository _repository;
        private readonly IGitHubClient _client;
        private readonly CommandLineOptions _options;
        private readonly ILogger<RemoteGitRepository> _logger;

        public string CloneUrl => _repository.CloneUrl;
        public string Owner => _repository.Owner.Login;
        public string Name => _repository.Name;
        public string Branch => _repository.DefaultBranch;

        public RemoteGitRepository(Repository repository, IGitHubClient client, CommandLineOptions options, ILogger<RemoteGitRepository> logger)
        {
            _repository = repository;
            _client = client;
            _options = options;
            _logger = logger;
        }

        public async Task<IRemoteGitRepository> ForkRepositoryAsync()
        {
            _logger.LogInformation("Forking {Repository}", _repository.FullName);

            var repository = await _client.Repository.Forks.Create(_repository.Owner.Login, _repository.Name, new NewRepositoryFork());

            _logger.LogInformation("Created fork {Repository}", repository.FullName);

            return new RemoteGitRepository(repository, _client, _options, _logger);
        }

        public ILocalGitRepository CheckoutRepository()
        {
            var co = new CloneOptions
            {
                CredentialsProvider = CreateCredentials
            };

            var dir = Path.Combine(Path.GetTempPath(), "git-repositories", _repository.Owner.Login, _repository.Name);

            CleanupRepository(dir);

            var sourceUrl = $"https://github.com/{_repository.Owner.Login}/{_repository.Name}.git";

            _logger.LogInformation("Cloning {CloneUrl} into {Directory}", sourceUrl, dir);

            var path = LibGit2Sharp.Repository.Clone(sourceUrl, dir, co);

            var localRepository = new LibGit2Sharp.Repository(path);

            return new LocalGitRepository(localRepository, new PhysicalDirectoryInfo(new DirectoryInfo(dir)),   _options, _client, this);
        }

        private void CleanupRepository(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            _logger.LogInformation("{Directory} already exists, cleaning up", dir);

            NormalizeAttributes(dir);

            Directory.Delete(dir, true);
        }

        private static void NormalizeAttributes(string directoryPath)
        {
            var filePaths = Directory.GetFiles(directoryPath);
            var subDirectoryPaths = Directory.GetDirectories(directoryPath);

            foreach (var filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            foreach (var subDirectoryPath in subDirectoryPaths)
            {
                NormalizeAttributes(subDirectoryPath);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
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