using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;
using UpDock.Git;
using UpDock.Nodes;
using UpDock.Registry;
using Microsoft.Extensions.Logging;
using Octokit;

namespace UpDock
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
            try
            {
                var repositories = await _client.Search.SearchRepo(new SearchRepositoriesRequest(_options.Search));

                foreach (var repository in repositories.Items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _logger.LogStartedProcessingRepository(repository);

                    try
                    {
                        await ProcessRepositoryAsync(repository, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception");
                    }

                    _logger.LogFinishedProcessingRepository(repository);
                }
            }
            catch(ApiValidationException ex)
            {
                _logger.LogError(ex, "Unable to authenticate with GitHub, please make sure the token being used is valid for the search you are doing.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
}

        private async Task ProcessRepositoryAsync(Repository repository, CancellationToken cancellationToken)
        {
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

            if (replacements.Count == 0)
            {
                _logger.LogNoChangesInRepository(repository);
            }

            foreach (var replacement in replacements.GroupBy(x => x.Group))
            {
                var groupedReplacements = replacement.ToList();

                localRepository.Reset();

                await _executor.ExecutePlanAsync(groupedReplacements, cancellationToken);

                if (!localRepository.IsDirty)
                    continue;

                _logger.LogChangesDetectedInRepository(repository);

                if (_options.DryRun)
                    continue;

                var forkedRepository = await gitRepository.ForkRepositoryAsync();

                await localRepository.CreatePullRequestAsync(forkedRepository, groupedReplacements, cancellationToken);
            }
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
            var file = directory.GetFile("up-dock.json");

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
