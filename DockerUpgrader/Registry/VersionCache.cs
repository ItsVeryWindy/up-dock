using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DockerUpgrader.Imaging;
using DockerUpgrader.Nodes;
using DockerUpgrader.Registry;
using Microsoft.Extensions.Logging;

namespace DockerUpgrader
{
    public class VersionCache : IVersionCache
    {
        private readonly HttpClient _client;
        private readonly ILogger<VersionCache> _logger;
        private readonly IConfigurationOptions _options;
        private readonly ConcurrentDictionary<(Uri, string), TagList> _tagLists = new ConcurrentDictionary<(Uri, string), TagList>();
        private readonly ConcurrentDictionary<(Uri, string), AuthToken> _authTokens = new ConcurrentDictionary<(Uri, string), AuthToken>();

        public VersionCache(HttpClient client, ILogger<VersionCache> logger, IConfigurationOptions options)
        {
            _client = client;
            _logger = logger;
            _options = options;
        }

        public async Task UpdateCacheAsync(IEnumerable<DockerImageTemplatePattern> patterns, CancellationToken cancellationToken)
        {
            var tagListTasks = patterns
                .Select(x => x.Template)
                .Select(x => (x.Repository, x.Image))
                .Distinct()
                .Select(x => UpdateTagsAsync(x.Repository, x.Image, cancellationToken));

            await Task.WhenAll(tagListTasks);
        }

        private async Task UpdateTagsAsync(Uri repository, string image, CancellationToken cancellationToken)
        {
            if(_tagLists.ContainsKey((repository, image)))
                return;

            try
            {
                var tags = await RequestTags(repository, image, cancellationToken);

                _tagLists[(repository, image)] = tags;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not download tags for {Repository}/{Image}, skipping", repository, image);
            }
        }

        private async Task<TagList> RequestTags(Uri repository, string image, CancellationToken cancellationToken)
        {
            var url = new Uri(repository, $"v2/{image}/tags/list");

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_options.Authentication.TryGetValue(repository.Host, out var authenticationOptions))
            {
                var base64 = Convert.ToBase64String(Encoding.GetBytes($"{authenticationOptions.Username}:{authenticationOptions.Password}"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            }
            else
            {
                var token = GetExistingToken(repository, image);

                if (token != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                }
            }

            var response = await _client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var token = await CreateAuthTokenAsync(response, cancellationToken);

                if (token != null)
                {
                    _authTokens[(repository, image)] = token;
                }

                return await RequestTags(repository, image, cancellationToken);
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TagList>(content);
        }

        private AuthToken? GetExistingToken(Uri repository, string image)
        {
            if(!_authTokens.TryGetValue((repository, image), out var token))
                return null;

            return token.Expired(DateTimeOffset.UtcNow) ? null : token;
        }

        private static readonly Encoding Encoding = Encoding.GetEncoding("ISO-8859-1");

        private async Task<AuthToken?> CreateAuthTokenAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var authHeader = response.Headers.WwwAuthenticate.FirstOrDefault();

            if (authHeader == null)
                return null;

            var parsedHeader = ParseWWWAuthenticate(authHeader);

            if(parsedHeader == null || !parsedHeader.TryGetValue("realm", out var realm))
                return null;

            var queryString = string.Join('&',
                parsedHeader
                    .Where(kv => kv.Key != "realm")
                    .Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));

            var builder = new UriBuilder(realm);

            builder.Query = queryString;

            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

            var response2 = await _client.SendAsync(request, cancellationToken);

            if (!response2.IsSuccessStatusCode)
                return null;

            var content = await response2.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AuthToken>(content);
        }

        private Dictionary<string,string>? ParseWWWAuthenticate(AuthenticationHeaderValue authHeader)
        {
            var dictionary = new Dictionary<string, string>();

            var span = authHeader.Parameter.AsSpan();

            return ReadKey(span, 1, span, dictionary) ? dictionary : null;
        }

        private bool ReadKey(ReadOnlySpan<char> key, int length, ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if(span.IsEmpty)
                return false;

            var next = span.Slice(1);

            return ReadEquals(key, length, next, dictionary) || ReadKey(key, length + 1, next, dictionary);
        }

        private bool ReadEquals(ReadOnlySpan<char> key, int length, ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if(span.IsEmpty)
                return false;
            
            if(span[0] != '=')
                return false;

            return ReadBeginQuote(key.Slice(0, length), span.Slice(1), dictionary);
        }

        private bool ReadBeginQuote(ReadOnlySpan<char> key, ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if (span.IsEmpty)
                return false;

            if(span[0] != '\"')
                return false;

            var next = span.Slice(1);

            return ReadValue(key, next, 1, next, dictionary);
        }

        private bool ReadValue(ReadOnlySpan<char> key, ReadOnlySpan<char> value, int length, ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if(span.IsEmpty)
                return false;

            var next = span.Slice(1);

            return ReadEndQuote(key, value, length, next, dictionary) || ReadValue(key, value, length + 1, next, dictionary);
        }

        private bool ReadEndQuote(ReadOnlySpan<char> key, ReadOnlySpan<char> value, int length, ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if (span.IsEmpty)
                return false;

            if(span[0] != '\"')
                return false;

            dictionary[key.ToString()] = value.Slice(0, length).ToString();

            return ReadComma(span.Slice(1), dictionary);
        }

        private bool ReadComma(ReadOnlySpan<char> span, Dictionary<string, string> dictionary)
        {
            if (span.IsEmpty)
                return true;

            if(span[0] != ',')
                return false;

            var next = span.Slice(1);

            return ReadKey(next, 1, next, dictionary);
        }

        public DockerImagePattern? FetchLatest(DockerImagePattern pattern)
        {
            if (!_tagLists.TryGetValue((pattern.Image.Repository, pattern.Image.Image), out var tagList))
                return null;

            var builder = new SearchNodeBuilder();

            builder.Add(pattern.Image.Template.CreatePattern(false, false));

            var node = builder.Build();

            var versions = FindMatchingDockerImages(node, tagList);

            var matchingVersion = versions.LastOrDefault(pattern.Image.Template.Satisfies);

            return matchingVersion == null ? null : pattern.Create(matchingVersion);
        }

        private static IEnumerable<DockerImage> FindMatchingDockerImages(ISearchTreeNode node, TagList tagList)
        {
            var versions = new List<DockerImage>();

            foreach(var tag in tagList.Tags)
            {
                var result = node.Search(tag);

                if(result.Pattern != null)
                {
                    versions.Add(result.Pattern.Image);
                }
            }

            versions.Sort();

            return versions;
        }
    }
}