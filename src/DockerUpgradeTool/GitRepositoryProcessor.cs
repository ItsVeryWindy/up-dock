using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DockerUpgradeTool.Files;
using DockerUpgradeTool.Git;
using DockerUpgradeTool.Nodes;
using DockerUpgradeTool.Registry;
using Microsoft.Extensions.Logging;
using Octokit;

namespace DockerUpgradeTool
{
    public class GitRepositoryProcessor : IGitRepositoryProcessor
    {
        private readonly IGitHubClient _client;
        private readonly IGitRepositoryFactory _factory;
        private readonly IReplacementPlanner _planner;
        private readonly IReplacementPlanExecutor _executor;
        private readonly IConfigurationOptions _options;
        private readonly IFileProvider _fileProvider;
        private readonly IFileFilterFactory _fileFilterFactory;
        private readonly ICancellationProvider _cancellationProvider;
        private readonly IVersionCache _cache;
        private readonly ILogger<GitRepositoryProcessor> _logger;

        public GitRepositoryProcessor(IGitHubClient client, IGitRepositoryFactory factory, IReplacementPlanner planner, IReplacementPlanExecutor executor, IConfigurationOptions options, IFileProvider fileProvider, IFileFilterFactory fileFilterFactory, ICancellationProvider cancellationProvider, IVersionCache cache, ILogger<GitRepositoryProcessor> logger)
        {
            _client = client;
            _factory = factory;
            _planner = planner;
            _executor = executor;
            _options = options;
            _fileProvider = fileProvider;
            _fileFilterFactory = fileFilterFactory;
            _cancellationProvider = cancellationProvider;
            _cache = cache;
            _logger = logger;
        }

        public async Task ProcessAsync()
        {
            var repositories = await _client.Search.SearchRepo(new SearchRepositoriesRequest(_options.Search));

            foreach (var repository in repositories.Items)
            {
                await ProcessRepositoryAsync(repository);
            }
        }

        private async Task ProcessRepositoryAsync(Repository repository)
        {
            _cancellationProvider.CancellationToken.ThrowIfCancellationRequested();

            var gitRepository = _factory.CreateRepository(repository);

            var localRepository = gitRepository.CheckoutRepository();

            var directory = _fileProvider.GetDirectory(localRepository.WorkingDirectory);

            var localOptions = GetConfiguration(directory);

            await _cache.UpdateCacheAsync(localOptions.Patterns, _cancellationProvider.CancellationToken);

            var filter = _fileFilterFactory.Create(localOptions);

            var replacements = new List<TextReplacement>();

            var builder = new SearchNodeBuilder();

            foreach (var pattern in localOptions.Patterns)
            {
                builder.Add(pattern);
            }

            var node = builder.Build();

            foreach (var file in directory.Files)
            {
                await ProcessFile(filter, localRepository, file, node, replacements);
            }

            foreach (var replacement in replacements.GroupBy(x => x.Group))
            {
                var groupedReplacements = replacement.ToList();

                localRepository.Reset();

                await _executor.ExecutePlanAsync(groupedReplacements, _cancellationProvider.CancellationToken);

                if (!localRepository.IsDirty)
                    continue;

                _logger.LogInformation("Changes detected in repository {Repository}", repository.FullName);

                var forkedRepository = await gitRepository.ForkRepositoryAsync();

                await localRepository.CreatePullRequestAsync(forkedRepository, groupedReplacements, _cancellationProvider.CancellationToken);
            }
        }

        private async Task ProcessFile(IFileFilter filter, ILocalGitRepository repository, IFileInfo file, ISearchTreeNode node, List<TextReplacement> replacements)
        {
            _cancellationProvider.CancellationToken.ThrowIfCancellationRequested();

            if(!filter.Filter(repository, file))
                return;

            var plan = await _planner.GetReplacementPlanAsync(file, node, _cancellationProvider.CancellationToken);

            if (plan.Count > 0)
            {
                replacements.AddRange(plan);
            }
        }

        private IConfigurationOptions GetConfiguration(IDirectoryInfo directory)
        {
            var localConfigPath = Path.Join(directory.Path, "docker_images.json");

            var file = _fileProvider.GetFile(localConfigPath);

            if (file?.Exists == true)
            {
                using var stream = file.CreateReadStream();

                var localOptions = new ConfigurationOptions();
                
                localOptions.Populate(stream);
                
                return _options.Merge(localOptions);
            }

            return _options;
        }
    }
}
