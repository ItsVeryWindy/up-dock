﻿using System;
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
                    var (image, endIndex) = node.Search(line.AsSpan().Slice(i));

                    if (image == null)
                    {
                        i++;
                        continue;
                    }

                    var currentVersion = line.Substring(i, endIndex);

                    var latestPattern = _cache.FetchLatest(image);

                    var latestVersion = latestPattern?.ToString();

                    _logger.LogIdentifiedFromPattern(currentVersion, lineNumber + 1, file, image);

                    if (latestVersion != null && ((allowDowngrade && currentVersion != latestVersion) || (!allowDowngrade && latestPattern!.Image.CompareTo(image.Image) > 0)))
                    {
                        _logger.LogReplacingOutdatedVersion(currentVersion, latestVersion);

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
