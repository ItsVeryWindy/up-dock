using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UpDock.Files;
using UpDock.Git.Drivers;
using UpDock.Tests.Stubs;

namespace UpDock.Tests
{
    [TestFixture(typeof(GitProcessDriver), typeof(PhysicalFileProvider))]
    [TestFixture(typeof(LibGit2SharpDriver), typeof(PhysicalFileProvider))]
    [TestFixture(typeof(StubGitDriver), typeof(StubFileProvider))]
    public class GitDriverTests<TDriver, TFileProvider>
        where TDriver : class, IGitDriver
        where TFileProvider : class, IFileProvider
    {
        private IServiceProvider _sp = null!;
        private IDirectoryInfo _remoteDirectory = null!;
        private TDriver _driver = null!;
        private string _cloneUrl;
        private IDirectoryInfo _cloneDirectory = null!;
        private IDirectoryInfo _separateCloneDirectory = null!;

        [SetUp]
        public async Task SetUp()
        {
            _sp = TestUtilities
                .CreateServices()
                .AddSingleton<TDriver>()
                .AddSingleton<IFileProvider, TFileProvider>()
                .BuildServiceProvider();

            _remoteDirectory = _sp.GetRequiredService<IFileProvider>().CreateTemporaryDirectory();

            _driver = _sp.GetRequiredService<TDriver>();

            _cloneUrl = await _driver.CreateRemoteAsync(_remoteDirectory, CancellationToken.None);

            _cloneDirectory = _sp.GetRequiredService<IFileProvider>().CreateTemporaryDirectory();

            _separateCloneDirectory = _sp.GetRequiredService<IFileProvider>().CreateTemporaryDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            NormalizeAttributes(_remoteDirectory);
            NormalizeAttributes(_cloneDirectory);
            NormalizeAttributes(_separateCloneDirectory);

            _remoteDirectory?.Delete();
            _cloneDirectory?.Delete();
            _separateCloneDirectory?.Delete();
        }

        [Test]
        public async Task ShouldCommitAndPushFilesToRemoteRepository()
        {
            const string committedFileName = "my-file";
            const string ignoredFileName = "my-ignored-file";

            using var repository = await _driver.CloneAsync(_cloneUrl, _cloneDirectory, null, CancellationToken.None);

            var head = await repository.GetHeadAsync(CancellationToken.None);

            Assert.That(head.FullName, Is.EqualTo("refs/heads/master"));
            Assert.That(head.Name, Is.EqualTo("master"));

            var remotes = await repository.GetRemotesAsync(CancellationToken.None);

            Assert.That(remotes, Has.Count.EqualTo(1));

            var remote = remotes.First();

            Assert.That(remote.Name, Is.EqualTo("origin"));

            _cloneDirectory.GetFile(committedFileName).CreateWriteStream().Dispose();

            using var stream = _cloneDirectory.GetFile(".gitignore").CreateWriteStream();

            using var sw = new StreamWriter(stream);

            sw.WriteLine(ignoredFileName);

            sw.Dispose();

            _cloneDirectory.GetFile(ignoredFileName).CreateWriteStream().Dispose();

            var fileToStage = repository.Files.FirstOrDefault(x => x.RelativePath == committedFileName);

            Assert.That(fileToStage, Is.Not.Null);

            await fileToStage!.StageAsync(CancellationToken.None);

            await repository.CommitAsync("init commit", "email@address.com", CancellationToken.None);

            var trackedHead = await head.TrackAsync(remote, CancellationToken.None);

            await trackedHead.PushAsync(CancellationToken.None);

            var ignoredFile = repository.Files.FirstOrDefault(x => x.RelativePath == ignoredFileName);

            Assert.That(ignoredFile, Is.Not.Null);
            Assert.That(await ignoredFile!.IsIgnoredAsync(CancellationToken.None), Is.True);

            var references = await remote.GetReferencesAsync(CancellationToken.None);

            var reference = references.First();

            Assert.That(reference.FullName, Is.EqualTo("refs/heads/master"));

            var branch = await repository.CreateBranchAsync("my-branch", CancellationToken.None);

            await branch.CheckoutAsync(CancellationToken.None);

            var trackedBranch = await branch.TrackAsync(remote, CancellationToken.None);

            await trackedBranch.PushAsync(CancellationToken.None);

            var branches = await repository.GetBranchesAsync(CancellationToken.None);

            Assert.That(branches, Has.Count.EqualTo(4));

            GitDriverTests<TDriver, TFileProvider>.AssertBranch(branches.First(), "refs/heads/master", "master", false);
            GitDriverTests<TDriver, TFileProvider>.AssertBranch(branches.Skip(1).First(), "refs/heads/my-branch", "my-branch", false);
            GitDriverTests<TDriver, TFileProvider>.AssertBranch(branches.Skip(2).First(), "refs/remotes/origin/master", "origin/master", true);
            GitDriverTests<TDriver, TFileProvider>.AssertBranch(branches.Skip(3).First(), "refs/remotes/origin/my-branch", "origin/my-branch", true);

            using var separateRepository = await _driver.CloneAsync(_remoteDirectory.AbsolutePath, _separateCloneDirectory, null, CancellationToken.None);

            var separateFile = separateRepository.Files.FirstOrDefault(x => x.RelativePath == committedFileName);

            Assert.That(separateFile, Is.Not.Null);

            using var separateStream = separateFile!.File.CreateWriteStream();

            using var separateSW = new StreamWriter(separateStream);

            separateSW.WriteLine(committedFileName);

            separateSW.Dispose();

            var isDirty = await separateRepository.IsDirtyAsync(CancellationToken.None);

            Assert.That(isDirty, Is.True);
        }

        private static void AssertBranch(IBranch branch, string fullName, string name, bool isRemote)
        {
            Assert.That(branch.FullName, Is.EqualTo(fullName));
            Assert.That(branch.Name, Is.EqualTo(name));
            Assert.That(branch.IsRemote, Is.EqualTo(isRemote));
        }

        private static void NormalizeAttributes(IDirectoryInfo? directory)
        {
            if (directory is null)
                return;

            foreach (var file in directory.Files)
            {
                file.SetAttributes(FileAttributes.Normal);
            }

            foreach (var subDirectory in directory.Directories)
            {
                NormalizeAttributes(subDirectory);
            }

            directory.SetAttributes(FileAttributes.Normal);
        }
    }
}
