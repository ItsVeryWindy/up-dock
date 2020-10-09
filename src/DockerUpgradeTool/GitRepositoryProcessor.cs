using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly IFileFilterFactory _fileFilterFactory;
        private readonly IVersionCache _cache;
        private readonly ILogger<GitRepositoryProcessor> _logger;

        public GitRepositoryProcessor(IGitHubClient client, IGitRepositoryFactory factory, IReplacementPlanner planner, IReplacementPlanExecutor executor, IConfigurationOptions options, IFileFilterFactory fileFilterFactory, IVersionCache cache, ILogger<GitRepositoryProcessor> logger)
        {
            _client = client;
            _factory = factory;
            _planner = planner;
            _executor = executor;
            _options = options;
            _fileFilterFactory = fileFilterFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            var repositories = await _client.Search.SearchRepo(new SearchRepositoriesRequest(_options.Search));

            foreach (var repository in repositories.Items)
            {
                await ProcessRepositoryAsync(repository, cancellationToken);
            }
        }

        private async Task ProcessRepositoryAsync(Repository repository, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Started processing repository {Repository}", repository.FullName);

            var gitRepository = _factory.CreateRepository(repository);

            var localRepository = gitRepository.CheckoutRepository();

            var directory = localRepository.Directory;

            var localOptions = GetConfiguration(directory);

            await _cache.UpdateCacheAsync(localOptions.Patterns, cancellationToken);

            var filter = _fileFilterFactory.Create(localOptions);

            var replacements = new List<TextReplacement>();

            var builder = new SearchNodeBuilder();

            foreach (var pattern in localOptions.Patterns)
            {
                builder.Add(pattern);
            }

            var node = builder.Build();

            foreach (var file in localRepository.Files)
            {
                await ProcessFileAsync(filter, file, node, replacements, cancellationToken);
            }

            if(replacements.Count == 0)
            {
                _logger.LogInformation("No changes to be made in repository {Repository}", repository.FullName);
            }

            foreach (var replacement in replacements.GroupBy(x => x.Group))
            {
                var groupedReplacements = replacement.ToList();

                localRepository.Reset();

                await _executor.ExecutePlanAsync(groupedReplacements, cancellationToken);

                if (!localRepository.IsDirty)
                    continue;

                _logger.LogInformation("Changes detected in repository {Repository}", repository.FullName);

                if (_options.DryRun)
                    continue;

                var forkedRepository = await gitRepository.ForkRepositoryAsync();

                await localRepository.CreatePullRequestAsync(forkedRepository, groupedReplacements, cancellationToken);
            }

            _logger.LogInformation("Finished processing repository {Repository}", repository.FullName);
        }

        private async Task ProcessFileAsync(IFileFilter filter, IRepositoryFileInfo file, ISearchTreeNode node, List<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(!filter.Filter(file))
                return;

            var plan = await _planner.GetReplacementPlanAsync(file, node, cancellationToken);

            if (plan.Count > 0)
            {
                replacements.AddRange(plan);
            }
        }

        private IConfigurationOptions GetConfiguration(IDirectoryInfo directory)
        {
            var file = directory.GetFile("docker_images.json");

            if (file.Exists == true)
            {
                using var stream = file.CreateReadStream();

                if(stream == null)
                    throw new InvalidOperationException("Could not read the configuration file in the repository");

                var localOptions = new ConfigurationOptions();
                
                localOptions.Populate(stream);
                
                return _options.Merge(localOptions);
            }

            return _options;
        }
    }
}
