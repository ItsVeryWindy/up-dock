using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Octokit;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Imaging;
using UpDock.Nodes;
using UpDock.Registry;

namespace UpDock.Caching
{
    public class UpdateCache : IUpdateCache
    {
        private readonly CommandLineOptions _options;

        private readonly Dictionary<string, UpdateCacheEntry> _repositories;
        private readonly IVersionCache _versionCache;
        private readonly IFileProvider _provider;

        public UpdateCache(CommandLineOptions options, IVersionCache versionCache, IFileProvider provider)
        {
            _options = options;
            _repositories = new();
            _versionCache = versionCache;
            _provider = provider;
        }

        public bool HasChanged(IRepository repository, IConfigurationOptions options)
        {
            if (_options.Cache is null)
                return true;

            var hash = CreateHash(repository, options);

            if (!_repositories.TryGetValue(repository.CloneUrl, out var entry))
                return true;

            return entry.HasChanged(hash);
        }

        private static readonly SHA256 Sha256Hash = SHA256.Create();

        private static string CreateHash(IRepository repository, IConfigurationOptions options)
        {
            var hash = options.CreateHash();

            return Sha256Hash.ComputeHash($"{hash}:{repository.PushedAt:u}");
        }

        public void Set(IRepository repository, IConfigurationOptions options, IEnumerable<DockerImage?> images)
        {
            if (_options.Cache is null)
                return;

            var hash = CreateHash(repository, options);

            var entry = new UpdateCacheEntry(_versionCache, hash);

            foreach (var image in images)
            {
                if (image is null)
                    return;

                entry.Images.Add(image);
            }

            _repositories[repository.CloneUrl] = entry;
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            using var doc = await ParseDocumentAsync(cancellationToken);

            if (doc is null)
                return;

            if (!doc.RootElement.TryGetProperty("images", out var imagesElement) || imagesElement.ValueKind != JsonValueKind.Object)
                return;

            var references = new List<DockerImage>();

            foreach(var property in imagesElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                    return;

                var image = ParseImage(property.Name, property.Value.GetString());

                if (image is null)
                    return;

                references.Add(image);
            }

            if (!doc.RootElement.TryGetProperty("repositories", out var repositoriesElement) || repositoriesElement.ValueKind != JsonValueKind.Object)
                return;

            var repositories = new Dictionary<string, UpdateCacheEntry>();

            foreach (var property in repositoriesElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object)
                    return;

                if (!property.Value.TryGetProperty("hash", out var hashElement) || hashElement.ValueKind != JsonValueKind.String)
                    return;

                var hash = hashElement.GetString()!;

                if (!property.Value.TryGetProperty("entries", out var entriesElement) || entriesElement.ValueKind != JsonValueKind.Array)
                    return;

                var entry = new UpdateCacheEntry(_versionCache, hash);

                foreach (var entryElement in entriesElement.EnumerateArray())
                {
                    if (entryElement.ValueKind != JsonValueKind.Number)
                        return;

                    if (!entryElement.TryGetInt32(out var index))
                        return;

                    if (index < 0)
                        return;

                    if (index >= references.Count)
                        return;

                    entry.Images.Add(references[index]);
                }

                repositories.Add(property.Name, entry);
            }

            foreach(var repository in repositories)
            {
                _repositories.Add(repository.Key, repository.Value);
            }
        }

        private static DockerImage? ParseImage(string template, string? image)
        {
            if (image is null)
                return null;

            try
            {
                return new SearchNodeBuilder()
                    .Add(DockerImageTemplate.Parse(template).CreatePattern(true, true, true, true, false))
                    .Build()
                    .Search(image)
                    .Pattern?
                    .Image;
            }
            catch(FormatException)
            {
                return null;
            }
        }

        private async Task<JsonDocument?> ParseDocumentAsync(CancellationToken cancellationToken)
        {
            var file = _provider.GetFile(_options.Cache);

            if (file is null)
                return null;

            await using var stream = file.CreateReadStream();

            if (stream is null)
                return null;

            try
            {
                return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            }
            catch(JsonException)
            {
                return null;
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (_options.Cache is null)
                return;

            var references =_repositories
                .Values
                .SelectMany(x => x.Images)
                .Distinct()
                .ToList();

            var file = _provider.GetFile(_options.Cache)!;

            await using var stream = file.CreateWriteStream();

            await using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();

            writer.WriteStartObject("images");
            
            foreach(var image in references)
            {
                writer.WriteString(image.Template.ToString(), image.ToString());
            }

            writer.WriteEndObject();

            writer.WriteStartObject("repositories");

            foreach (var (repository, entry) in _repositories)
            {
                writer.WriteStartObject(repository.ToString());

                writer.WriteString("hash", entry.Hash);

                writer.WriteStartArray("entries");

                foreach(var index in references.Select((x, i) => (x, i)).Where(x => entry.Images.Contains(x.x)).Select(x => x.i))
                {
                    writer.WriteNumberValue(index);
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            writer.WriteEndObject();

            writer.WriteEndObject();

            await writer.FlushAsync(cancellationToken);
        }
    }
}
