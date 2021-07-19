using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UpDock.CommandLine;
using UpDock.Imaging;
using UpDock.Nodes;
using UpDock.Registry;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace UpDock.Tests
{
    public class ReplacementPlannerTests
    {
        [TestCaseSource(nameof(PositiveTestCases))]
        public async Task ShouldReturnLinesToBeReplaced(DockerImageTemplatePattern pattern, string fileName, string expectedFrom, int expectedLineNumber, int expectedStart, string expectedTo, bool allowDowngrade)
        {
            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            await sp.GetRequiredService<IVersionCache>().UpdateCacheAsync(Enumerable.Repeat(pattern, 1), CancellationToken.None);

            var node = new SearchNodeBuilder().Add(pattern).Build();

            var planner = sp.GetRequiredService<IReplacementPlanner>();

            var stream = TestUtilities.GetResource($"Files.{fileName}")!;

            var fileInfo = new StubFileInfo(stream, "/file/path");

            var results = await planner.GetReplacementPlanAsync(fileInfo, node, allowDowngrade, CancellationToken.None);

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results.First().From, Is.EqualTo(expectedFrom));
            Assert.That(results.First().LineNumber, Is.EqualTo(expectedLineNumber));
            Assert.That(results.First().Start, Is.EqualTo(expectedStart));
            Assert.That(results.First().To, Is.EqualTo(expectedTo));
        }

        [TestCaseSource(nameof(NegativeTestCases))]
        public async Task ShouldNotReplaceLines(DockerImageTemplatePattern pattern, string fileName, bool allowDowngrade)
        {
            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<HttpMessageHandler>(new StaticResponseHandler())
                .AddSingleton<CommandLineOptions>()
                .BuildServiceProvider();

            await sp.GetRequiredService<IVersionCache>().UpdateCacheAsync(Enumerable.Repeat(pattern, 1), CancellationToken.None);

            var node = new SearchNodeBuilder().Add(pattern).Build();

            var planner = sp.GetRequiredService<IReplacementPlanner>();

            var stream = TestUtilities.GetResource($"Files.{fileName}")!;

            var fileInfo = new StubFileInfo(stream, "/file/path");

            var results = await planner.GetReplacementPlanAsync(fileInfo, node, allowDowngrade, CancellationToken.None);

            Assert.That(results, Has.Count.EqualTo(0));
        }

        public static IEnumerable<TestCaseData> PositiveTestCases
        {
            get
            {
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern(true, true, true),
                    "Dockerfile", "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", 0, 5, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", true);
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern("sdk:{v}-alpine{v}"),
                    "another_file.txt", "sdk:3.1.101-alpine3.10", 8, 20, "sdk:3.1.102-alpine3.11", true);
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern(true, true, true),
                    "Dockerfile_higher", "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.12", 0, 5, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", true);
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern("mcr.microsoft.com/dotnet/core/sdk:{v3.1.*}-alpine{v}"),
                    "Dockerfile", "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", 0, 5, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", true);
            }
        }

        public static IEnumerable<TestCaseData> NegativeTestCases {
            get {
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern(true, true, true),
                    "Dockerfile_higher", false);
                yield return new TestCaseData(
                    DockerImageTemplate.Parse("mcr.microsoft.com/dotnet/core/sdk:{v}-alpine{v}").CreatePattern("mcr.microsoft.com/dotnet/core/sdk:{v3.0.*}-alpine{v}"),
                    "Dockerfile", true);
            }
        }
    }
}
