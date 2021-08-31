using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;
using UpDock.Git;
using UpDock.Nodes;
using UpDock.Registry;
using Microsoft.Extensions.Logging;
using Octokit;
using UpDock.Caching;

namespace UpDock
{
    public class GitRepositoryProcessor : IGitRepositoryProcessor
    {
        private readonly IRepositorySearcher _searcher;
        private readonly IReplacementPlanner _planner;
        private readonly IReplacementPlanExecutor _executor;
        private readonly IConfigurationOptions _options;
        private readonly IFileFilterFactory _fileFilterFactory;
        private readonly IVersionCache _cache;
        private readonly IUpdateCache _updateCache;
        private readonly ReportGenerator _reportGenerator;
        private readonly ILogger<GitRepositoryProcessor> _logger;

        public GitRepositoryProcessor(IRepositorySearcher searcher, IReplacementPlanner planner, IReplacementPlanExecutor executor, IConfigurationOptions options, IFileFilterFactory fileFilterFactory, IVersionCache cache, IUpdateCache updateCache, ReportGenerator reportGenerator, ILogger<GitRepositoryProcessor> logger)
        {
            _searcher = searcher;
            _planner = planner;
            _executor = executor;
            _options = options;
            _fileFilterFactory = fileFilterFactory;
            _cache = cache;
            _updateCache = updateCache;
            _reportGenerator = reportGenerator;
            _logger = logger;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                var repositories = await _searcher.SearchAsync(_options.Search!, cancellationToken);

                await _updateCache.LoadAsync(cancellationToken);

                foreach (var repository in repositories)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _logger.LogStartedProcessingRepository(repository);

                    try
                    {
                        if (_updateCache.HasChanged(repository, _options))
                        {
                            await ProcessRepositoryAsync(repository, cancellationToken);
                        }
                        else
                        {
                            _logger.LogSkippedProcessingRepository(repository);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception");
                    }

                    _logger.LogFinishedProcessingRepository(repository);
                }

                await _updateCache.SaveAsync(cancellationToken);

                await _reportGenerator.GenerateReportAsync(cancellationToken);
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

        private async Task ProcessRepositoryAsync(IRemoteGitRepository repository, CancellationToken cancellationToken)
        {
            using var localRepository = await repository.CheckoutRepositoryAsync(cancellationToken);

            var directory = localRepository.Directory;

            var localOptions = GetConfiguration(directory);

            await _cache.UpdateCacheAsync(localOptions.Patterns.Select(x => x.Template), cancellationToken);

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

                await localRepository.ResetAsync(cancellationToken);

                await _executor.ExecutePlanAsync(groupedReplacements, cancellationToken);

                var isDirty = await localRepository.IsDirtyAsync(cancellationToken);

                if (!isDirty)
                    continue;

                _logger.LogChangesDetectedInRepository(repository);

                if (_options.DryRun)
                    continue;

                var pullRequest = await localRepository.CreatePullRequestAsync(groupedReplacements, cancellationToken);

                if (pullRequest is not null)
                {
                    _reportGenerator.AddPullRequest(pullRequest.Value.url, pullRequest.Value.title);
                }
            }

            _updateCache.Set(repository, _options, localOptions.Patterns.Select(x => x.Template).Select(x => _cache.FetchLatest(x)));
        }

        private async Task ProcessFileAsync(IFileFilter filter, IRepositoryFileInfo file, ISearchTreeNode node, List<TextReplacement> replacements, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isFiltered = await filter.FilterAsync(file, cancellationToken);

            if (!isFiltered)
                return;

            var plan = await _planner.GetReplacementPlanAsync(file, node, _options.AllowDowngrade, cancellationToken);

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
