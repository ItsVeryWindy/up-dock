using System.IO;
using System.Threading.Tasks;
using UpDock.CommandLine;
using UpDock.Files;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Linq;
using System;

namespace UpDock.Git
{
    public class RemoteGitRepository : IRemoteGitRepository
    {
        private readonly Repository _repository;
        private readonly IGitHubClient _client;
        private readonly CommandLineOptions _options;
        private readonly IFileProvider _provider;
        private readonly ILogger<RemoteGitRepository> _logger;
        private readonly ILocalGitRepositoryFactory _factory;

        public string CloneUrl => _repository.CloneUrl;
        public string Owner => _repository.Owner.Login;
        public string Name => _repository.Name;
        public string DefaultBranch => _repository.DefaultBranch;
        public DateTimeOffset? PushedAt => _repository.PushedAt;
        public string FullName => _repository.FullName;

        public RemoteGitRepository(Repository repository, IGitHubClient client, CommandLineOptions options, IFileProvider provider, ILogger<RemoteGitRepository> logger, ILocalGitRepositoryFactory factory)
        {
            _repository = repository;
            _client = client;
            _options = options;
            _provider = provider;
            _logger = logger;
            _factory = factory;
        }

        public async Task<IRemoteGitRepository> ForkRepositoryAsync()
        {
            _logger.LogInformation("Forking {Repository}", _repository.FullName);

            var repository = await _client.Repository.Forks.Create(Owner, Name, new NewRepositoryFork());

            _logger.LogInformation("Created fork {Repository}", repository.FullName);

            return new RemoteGitRepository(repository, _client, _options, _provider, _logger, _factory);
        }

        public ILocalGitRepository CheckoutRepository()
        {
            var dir = Path.Combine(Path.GetTempPath(), "git-repositories", Owner, Name);

            return _factory.Create(CloneUrl, dir, this);
        }

        public async Task CreatePullRequestAsync(IRemoteGitRepository forkedRepository, PullRequest pullRequest)
        {
            try
            {
                var newPullRequest = new NewPullRequest(pullRequest.Title, $"{forkedRepository.Owner}:{pullRequest.Branch}", pullRequest.Branch)
                {
                    Body = pullRequest.Body
                };

                var createdPullRequest = await _client.PullRequest.Create(Owner, Name, newPullRequest);

                var labelUpdate = new IssueUpdate();

                labelUpdate.AddLabel("up-dock");

                _logger.LogInformation("Updating pull request {Url} with label", createdPullRequest.Url);

                await _client.Issue.Update(Owner, Name, createdPullRequest.Number, labelUpdate);
            }
            catch (ApiValidationException ex)
            {
                if (ex.ApiError.Errors.Any(x => x.Message.Contains("already exists") && x.Message.Contains(pullRequest.Branch)))
                {
                    _logger.LogInformation("Pull request already exists for branch {Branch} in repository {Repository}", pullRequest.Branch, CloneUrl);
                    return;
                }
                throw;
            }
        }
    }
}
