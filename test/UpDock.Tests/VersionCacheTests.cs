using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using NUnit.Framework;
using UpDock.CommandLine;
using UpDock.Imaging;
using UpDock.Registry;

namespace UpDock.Tests
{
    public class VersionCacheTests
    {
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v3.0.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.0.103-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v3.1.*}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        [TestCase("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11")]
        public async Task ShouldReturnCorrectLatestVersion(string templateStr, string latestStr)
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

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest!.ToString(), Is.EqualTo(latestStr));
        }
    }
}
