using Microsoft.Extensions.Logging;
using Octokit;

namespace DockerUpgrader.Git
{
    public class GitRepositoryFactory : IGitRepositoryFactory
    {
        private readonly IGitHubClient _client;
        private readonly CommandLineOptions _options;
        private readonly ILogger<RemoteGitRepository> _logger;

        public GitRepositoryFactory(IGitHubClient client, CommandLineOptions options, ILogger<RemoteGitRepository> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        public IRemoteGitRepository CreateRepository(Repository repository)
        {
            return new RemoteGitRepository(repository, _client, _options, _logger);
        }
    }
}
