using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.Git;
using DockerUpgradeTool.Nodes;
using DockerUpgradeTool.Registry;
using Microsoft.Extensions.Logging;

namespace DockerUpgradeTool
{
    public class ReplacementPlanner : IReplacementPlanner
    {
        private readonly IVersionCache _cache;
        private readonly ILogger<ReplacementPlanner> _logger;

        public ReplacementPlanner(IVersionCache cache, ILogger<ReplacementPlanner> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IRepositoryFileInfo file, ISearchTreeNode node, CancellationToken cancellationToken)
        {
            using var sr = new StreamReader(file.File.CreateReadStream());

            var replacements = new List<TextReplacement>();

            string? line;
            var lineNumber = 0;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                for (var i = 0; i < line.Length;)
                {
                    var (image, endIndex) = node.Search(line.AsSpan().Slice(i));

                    if (image == null)
                    {
                        i++;
                        continue;
                    }

                    var currentVersion = line.Substring(i, endIndex);

                    var latestPattern = _cache.FetchLatest(image);

                    var latestVersion = latestPattern?.ToString();

                    _logger.LogInformation("Identified '{CurrentVersion}' on line {LineNumber} of file {File}, from pattern {DockerImagePattern} with template {DockerImageTemplate}", currentVersion, lineNumber + 1, file.RelativePath, image.Pattern, image.Image.Template);

                    if (latestVersion != null && currentVersion != latestVersion.ToString())
                    {
                        _logger.LogInformation("Version is outdated replacing '{CurrentVersion}' with {LatestVersion}", currentVersion, latestVersion);

                        replacements.Add(new TextReplacement(image.Pattern.Group, file, currentVersion, image, latestVersion, latestPattern!, lineNumber, i));
                    }

                    i += endIndex;
                }

                lineNumber++;
            }

            return replacements;
        }
    }
}
