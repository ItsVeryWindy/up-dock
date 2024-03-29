﻿using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UpDock.CommandLine;
using UpDock.Imaging;
using UpDock.Nodes;
using UpDock.Registry;

namespace UpDock.Tests
{
    public class VersionCacheTests
    {
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v3.0.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.0.103-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v3.1.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        [TestCase("awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk:{v3.0.*}-alpine{v}", "awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk:3.0.103-alpine3.11")]
        [TestCase("dotnet/core/sdk:{v3.0.*}-alpine{v}", "dotnet/core/sdk:3.0.103-alpine3.11")]
        public async Task ShouldReturnCorrectLatestVersion(string templateStr, string latestStr)
        {
            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            sp.GetRequiredService<CommandLineOptions>().Authentication = new string[]
            {
                "awsaccountid.dkr.ecr.region.amazonaws.com=username,password"
            };

            var cache = sp.GetRequiredService<IVersionCache>();

            var template = DockerImageTemplate.Parse(templateStr);

            var expectedImage = CreateImage(template, latestStr);

            await cache.UpdateCacheAsync(Enumerable.Repeat(template, 1), CancellationToken.None);

            var latest = cache.FetchLatest(template);

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest!.ToString(), Is.EqualTo(expectedImage.ToString()));
            Assert.That(latest!.CompareTo(expectedImage), Is.EqualTo(0));
        }

        [TestCase("mcr.microsoft.com/dotnet/core/sdk@{digest}:{v3.0.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.0.103-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk@{digest}:{v3.1.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk@{digest}:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        [TestCase("awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk@{digest}:{v3.0.*}-alpine{v}", "awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk:{v}-alpine{v}", "awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk:3.0.103-alpine3.11")]
        [TestCase("dotnet/core/sdk@{digest}:{v3.0.*}-alpine{v}", "dotnet/core/sdk:{v}-alpine{v}", "dotnet/core/sdk:3.0.103-alpine3.11")]
        public async Task ShouldReturnCorrectLatestVersionFromDigest(string templateStr, string latestTemplateStr, string latestStr)
        {
            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            sp.GetRequiredService<CommandLineOptions>().Authentication = new string[]
            {
                "awsaccountid.dkr.ecr.region.amazonaws.com=username,password"
            };

            var cache = sp.GetRequiredService<IVersionCache>();

            var template = DockerImageTemplate.Parse(templateStr);

            var latestTemplate = DockerImageTemplate.Parse(latestTemplateStr);

            var expectedImage = CreateImage(latestTemplate, latestStr);

            await cache.UpdateCacheAsync(Enumerable.Repeat(template, 1), CancellationToken.None);

            var latest = cache.FetchLatest(template);

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest!.Digest, Is.EqualTo("sha256:4f880368ed63767483b6f6c5bf7efde3af3faba816e71ff42db50326b0386bed"));
            Assert.That(expectedImage.CompareTo(latest), Is.EqualTo(0));
        }

        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v3.0.*}-al")]
        public async Task ShouldNotReturnAVersion(string templateStr)
        {
            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            var cache = sp.GetRequiredService<IVersionCache>();

            var template = DockerImageTemplate.Parse(templateStr);

            await cache.UpdateCacheAsync(Enumerable.Repeat(template, 1), CancellationToken.None);

            var latest = cache.FetchLatest(template);

            Assert.That(latest, Is.Null);
        }

        [Test]
        public async Task ShouldNotReturnAVersionIfAuthenticationFailed()
        {
            var handler = new StaticResponseHandler();

            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(handler)
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            var cache = sp.GetRequiredService<IVersionCache>();

            var template = DockerImageTemplate.Parse("awsaccountid.dkr.ecr.region.amazonaws.com/dotnet/core/sdk:{v3.0.*}-alpine{v}");

            await cache.UpdateCacheAsync(Enumerable.Repeat(template, 1), CancellationToken.None);

            var latest = cache.FetchLatest(template);

            Assert.That(latest, Is.Null);
        }

        private static DockerImage CreateImage(DockerImageTemplate template, string search)
        {
            return new SearchNodeBuilder()
                    .Add(template.CreatePattern(true, true, true, false, false))
                    .Build()
                    .Search(search)
                    .Pattern?
                    .Image!;
        }
    }
}
