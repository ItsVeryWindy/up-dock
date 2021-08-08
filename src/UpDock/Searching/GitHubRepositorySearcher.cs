using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Git;

namespace UpDock
{
    public class GitHubRepositorySearcher : IRepositorySearcher
    {
        private readonly IGitHubClient _client;
        private readonly CommandLineOptions _options;
        private readonly IFileProvider _provider;
        private readonly ILogger<RemoteGitRepository> _logger;
        private readonly ILocalGitRepositoryFactory _factory;

        public GitHubRepositorySearcher(IGitHubClient client, CommandLineOptions options, IFileProvider provider, ILogger<RemoteGitRepository> logger, ILocalGitRepositoryFactory factory)
        {
            _client = client;
            _options = options;
            _provider = provider;
            _logger = logger;
            _factory = factory;
        }

        public async Task<IEnumerable<IRemoteGitRepository>> SearchAsync(string search, CancellationToken cancellationToken)
        {
            var result =  await _client.Search.SearchRepo(new SearchRepositoriesRequest(search));

            return result.Items.Select(x => new RemoteGitRepository(x, _client, _options, _provider, _logger, _factory));
        }
    }
}
