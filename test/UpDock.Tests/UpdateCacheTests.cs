using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Octokit;
using UpDock.Caching;
using UpDock.CommandLine;
using UpDock.Files;
using UpDock.Imaging;
using UpDock.Nodes;
using UpDock.Registry;

namespace UpDock.Tests
{
    public class UpdateCacheTests
    {
        private IUpdateCache _updateCache = null!;
        private IVersionCache _versionCache = null!;
        private StubFileProvider _provider = null!;
        private CommandLineOptions _options = null!;

        [SetUp]
        public void SetUp()
        {
            _provider = new StubFileProvider();
            _options = new CommandLineOptions
            {
                Cache = "/made/up/path"
            };

            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton(_options)
                .AddSingleton<IFileProvider>(_provider)
                .BuildServiceProvider();

            _updateCache = sp.GetRequiredService<IUpdateCache>();
            _versionCache = sp.GetRequiredService<IVersionCache>();
        }

        [Test]
        public async Task ShouldNotLoadIfCacheNotSpecified()
        {
            _options.Cache = null;

            await _updateCache.LoadAsync(CancellationToken.None);
        }

        [Test]
        public async Task ShouldNotLoadIfCacheFileNotFound()
        {
            await _updateCache.LoadAsync(CancellationToken.None);
        }

        [TestCase("{", Description = "Invalid json")]
        [TestCase("{}", Description = "No images property")]
        [TestCase("{\"images\": []}", Description = "Images property is not an object")]
        [TestCase("{\"images\": { \"abcd&654\": \"\" }}", Description = "Invalid docker image template")]
        [TestCase("{\"images\": { \"abcd\": [] }}", Description = "Docker image template property value is not a string")]
        [TestCase("{\"images\": { \"abcd\": \"\" }}", Description = "Invalid docker image")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }}", Description = "No repositories property")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": []}", Description = "Repositories property is not an object")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": []}}", Description = "Repository property is not an object")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {}}}", Description = "No hash property")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": []}}}", Description = "Hash property is not an string")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\"}}}", Description = "No entries property")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": {}}}}", Description = "Entries property is not an array")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [\"\"]}}}", Description = "Entries index is not a number")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [1.234]}}}", Description = "Entries index is not an integer")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [2147483648]}}}", Description = "Entries index is above max valid")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [1]}}}", Description = "Entries index is out of bounds")]
        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [-1]}}}", Description = "Entries index is negative")]
        public async Task ShouldNotLoadIfCacheFileIsInvalid(string contents)
        {
            _provider.AddFile(_options.Cache!, contents);

            Assert.That(() => _updateCache.LoadAsync(CancellationToken.None), Throws.Nothing);
            Assert.That(() => _updateCache.SaveAsync(CancellationToken.None), Throws.Nothing);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Cache!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo("{\"images\":{},\"repositories\":{}}"));
        }

        [TestCase("{\"images\": { \"abcd\": \"abcd:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [0]}}}", "{\"images\":{\"library/abcd:{v*}\":\"registry-1.docker.io/library/abcd:1234\"},\"repositories\":{\"my-repository\":{\"hash\":\"hash\",\"entries\":[0]}}}")]
        [TestCase("{\"images\": { \"abcd@{digest}:{v}\": \"abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec:1234\" }, \"repositories\": {\"my-repository\": {\"hash\": \"hash\", \"entries\": [0]}}}", "{\"images\":{\"library/abcd@{digest}:{v*}\":\"registry-1.docker.io/library/abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec:1234\"},\"repositories\":{\"my-repository\":{\"hash\":\"hash\",\"entries\":[0]}}}")]
        public async Task ShouldLoadIfCacheFileIsValid(string loaded, string saved)
        {
            _provider.AddFile(_options.Cache!, loaded);

            Assert.That(() => _updateCache.LoadAsync(CancellationToken.None), Throws.Nothing);
            Assert.That(() => _updateCache.SaveAsync(CancellationToken.None), Throws.Nothing);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Cache!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo(saved));
        }

        [Test]
        public async Task ShouldWriteToCache()
        {
            var image = CreateImage("1234");

            var options = new ConfigurationOptions();
            var repository = new StubRepository();

            Assume.That(options.CreateHash(), Is.EqualTo("8dc6e36ab1b53c503309e7e07ace0ba933cc11888166402a7993c9899995d6d0"));
            Assume.That(repository.PushedAt, Is.EqualTo(DateTimeOffset.MinValue));
            Assume.That(repository.PushedAt.ToString(), Is.EqualTo("01/01/0001 00:00:00 +00:00"));

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            Assert.That(() => _updateCache.SaveAsync(CancellationToken.None), Throws.Nothing);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Cache!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo("{\"images\":{\"library/abcd:{v*}\":\"registry-1.docker.io/library/abcd:1234\"},\"repositories\":{\"CloneUrl\":{\"hash\":\"9ada877545c28a3da684301f18e73588d0f516fafc9449bd706c2ec02e274806\",\"entries\":[0]}}}"));
        }

        [Test]
        public async Task ShouldWriteDigestToCache()
        {
            var image = CreateImage("abcd@{digest}", "abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec");

            var options = new ConfigurationOptions();
            var repository = new StubRepository();

            Assume.That(options.CreateHash(), Is.EqualTo("8dc6e36ab1b53c503309e7e07ace0ba933cc11888166402a7993c9899995d6d0"));
            Assume.That(repository.PushedAt, Is.EqualTo(DateTimeOffset.MinValue));
            Assume.That(repository.PushedAt.ToString(), Is.EqualTo("01/01/0001 00:00:00 +00:00"));

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            Assert.That(() => _updateCache.SaveAsync(CancellationToken.None), Throws.Nothing);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Cache!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo("{\"images\":{\"library/abcd@{digest}:{v*}\":\"registry-1.docker.io/library/abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec\"},\"repositories\":{\"CloneUrl\":{\"hash\":\"9ada877545c28a3da684301f18e73588d0f516fafc9449bd706c2ec02e274806\",\"entries\":[0]}}}"));
        }

        [Test]
        public async Task ShouldWriteDigestWithVersionToCache()
        {
            var image = new SearchNodeBuilder()
                    .Add(DockerImageTemplate.Parse("abcd@{digest}").CreatePattern("abcd@{digest}:{v}"))
                    .Build()
                    .Search("abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec:1234")
                    .Pattern?
                    .Image!;

            var options = new ConfigurationOptions();
            var repository = new StubRepository();

            Assume.That(options.CreateHash(), Is.EqualTo("8dc6e36ab1b53c503309e7e07ace0ba933cc11888166402a7993c9899995d6d0"));
            Assume.That(repository.PushedAt, Is.EqualTo(DateTimeOffset.MinValue));
            Assume.That(repository.PushedAt.ToString(), Is.EqualTo("01/01/0001 00:00:00 +00:00"));

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            Assert.That(() => _updateCache.SaveAsync(CancellationToken.None), Throws.Nothing);

            var newContents = await TestUtilities.GetStringAsync(_provider.GetFile(_options.Cache!).CreateReadStream());

            Assert.That(newContents, Is.EqualTo("{\"images\":{\"library/abcd@{digest}:{v*}\":\"registry-1.docker.io/library/abcd@sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bec:1234\"},\"repositories\":{\"CloneUrl\":{\"hash\":\"9ada877545c28a3da684301f18e73588d0f516fafc9449bd706c2ec02e274806\",\"entries\":[0]}}}"));
        }

        [Test]
        public async Task ShouldMarkAsChangedIfImageVersionIsDifferent()
        {
            var image = CreateImage("1234");

            var options = new ConfigurationOptions();

            var repository = new StubRepository();

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            await _versionCache.UpdateCacheAsync(Enumerable.Repeat(image.Template, 1), CancellationToken.None);

            Assert.That(_updateCache.HasChanged(repository, options), Is.True);
        }

        [Test]
        public async Task ShouldNotMarkAsChangedIfImageVersionIsTheSame()
        {
            var image = CreateImage("3.1.102");

            var options = new ConfigurationOptions();

            var repository = new StubRepository();

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            await _versionCache.UpdateCacheAsync(Enumerable.Repeat(image.Template, 1), CancellationToken.None);

            Assert.That(_updateCache.HasChanged(repository, options), Is.False);
        }

        [Test]
        public async Task ShouldMarkAsChangedIfConfigurationIsDifferent()
        {
            var image = CreateImage("1234");

            var options = new ConfigurationOptions
            {
                Patterns = { DockerImageTemplate.Parse("abcd").CreatePattern(true, true, true, false, true) }
            };

            var repository = new StubRepository();

            _updateCache.Set(repository, options, Enumerable.Repeat(image, 1));

            await _versionCache.UpdateCacheAsync(Enumerable.Repeat(image.Template, 1), CancellationToken.None);

            Assert.That(_updateCache.HasChanged(repository, options), Is.True);
        }

        private static DockerImage CreateImage(string version)
        {
            return CreateImage("abcd", $"abcd:{version}");
        }

        private static DockerImage CreateImage(string template, string search)
        {
            return new SearchNodeBuilder()
                    .Add(DockerImageTemplate.Parse(template).CreatePattern(true, true, true, false, false))
                    .Build()
                    .Search(search)
                    .Pattern?
                    .Image!;
        }
    }
}
