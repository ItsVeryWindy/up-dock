using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;
using UpDock.Nodes;
using UpDock.Registry;
using Microsoft.Extensions.Logging;

namespace UpDock
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

        public async Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IRepositoryFileInfo file, ISearchTreeNode node, bool allowDowngrade, CancellationToken cancellationToken)
        {
            await using var inputFileStream = file.File.CreateReadStream();

            if (inputFileStream == null)
                throw new InvalidOperationException($"Could not read the file {file.RelativePath}");

            using var sr = new StreamReader(inputFileStream);

            var replacements = new List<TextReplacement>();

            string? line;
            var lineNumber = 0;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                for (var i = 0; i < line.Length;)
                {
                    var (image, endIndex) = node.Search(line.AsSpan()[i..]);

                    if (image == null)
                    {
                        i++;
                        continue;
                    }

                    var currentVersion = line.Substring(i, endIndex);

                    _logger.LogIdentifiedFromPattern(currentVersion, lineNumber + 1, file, image);

                    var latestImage = _cache.FetchLatest(image.Pattern.Template);

                    var latestPattern = latestImage is null ? null : image.Create(latestImage);

                    var latestVersion = latestPattern?.ToString();

                    if (latestVersion is not null && latestPattern is not null && currentVersion != latestVersion && image.Image.CanUpgrade(latestImage, allowDowngrade))
                    {
                        _logger.LogReplacingOutdatedVersion(currentVersion, latestVersion);

                        replacements.Add(new TextReplacement(image.Pattern.Group, file, currentVersion, image, latestVersion, latestPattern, lineNumber, i));
                    }

                    i += endIndex;
                }

                lineNumber++;
            }

            return replacements;
        }
    }
}
