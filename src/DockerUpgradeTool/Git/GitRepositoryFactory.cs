using Microsoft.Extensions.Logging;
using Octokit;

namespace DockerUpgradeTool.Git
{
    public class GitRepositoryFactory : IGitRepositoryFactory
    {
        private readonly IGitHubClient _client;
        private readonly CommandLineOptions _options;
        private readonly ILogger<RemoteGitRepository> _remoteLogger;
        private readonly ILogger<LocalGitRepository> _localLogger;

        public GitRepositoryFactory(IGitHubClient client, CommandLineOptions options, ILogger<RemoteGitRepository> remoteLogger, ILogger<LocalGitRepository> localLogger)
        {
            _client = client;
            _options = options;
            _remoteLogger = remoteLogger;
            _localLogger = localLogger;
        }

        public IRemoteGitRepository CreateRepository(Repository repository) => new RemoteGitRepository(repository, _client, _options, _remoteLogger, _localLogger);
    }
}
