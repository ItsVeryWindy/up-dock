using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Files;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UpDock.Tests.Stubs;
using System.Linq;
using UpDock.Git;

namespace UpDock.Tests
{
    public class ReplacementPlanExecutorTests
    {
        [Test]
        public async Task ShouldPerformReplaceOnSingleFile()
        {
            var provider = new StubFileProvider();

            var file = await CreateFileAsync(provider, "Dockerfile");

            var sp = TestUtilities
                .CreateServices()
                .AddSingleton<IFileProvider>(provider)
                .BuildServiceProvider();

            var executor = sp.GetRequiredService<IReplacementPlanExecutor>();

            var replacements = new List<TextReplacement>
            {
                new TextReplacement("group", file, "mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10", null!, "mcr.microsoft.com/dotnet/core/sdk:3.1.102-alpine3.11", null!, 0, 5)
            };

            await executor.ExecutePlanAsync(replacements, CancellationToken.None);

            var replacedFile = await file.File.CreateReadStream().GetStringAsync();

            var expectedFile = await TestUtilities.GetResource("Files.Dockerfile_expected").GetStringAsync();

            Assert.That(replacedFile, Is.EqualTo(expectedFile));
        }

        private async Task<IRepositoryFileInfo> CreateFileAsync(StubFileProvider provider, string resource)
        {
            var driver = new StubGitDriver();

            var remoteDirectory = provider.GetDirectory("/remote").Create();

            await driver.CreateRemoteAsync(remoteDirectory, CancellationToken.None);

            var repository = await driver.CloneAsync(remoteDirectory.AbsolutePath, provider.GetDirectory("/clone"), null, CancellationToken.None);

            var stream = TestUtilities.GetResource($"Files.{resource}");

            var file = provider.GetFile("/clone/file/path");

            await stream.CopyToAsync(file.CreateWriteStream());

            return repository.Files.First();
        }
    }
}
