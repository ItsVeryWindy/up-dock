using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UpDock.CommandLine;
using NUnit.Framework;

namespace UpDock.Tests.CommandLine
{
    public class CommandLineOptionsRunnerTests
    {
        private StubConsoleWriter _writer = null!;
        private StubDisplayHelpInformation _displayHelpInformation = null!;
        private StubGitRepositoryProcessor _gitRepositoryProcessor = null!;
        private CommandLineOptionsRunner _commandLineOptionsRunner = null!;

        [SetUp]
        public void SetUp()
        {
            _writer = new StubConsoleWriter();

            _displayHelpInformation = new StubDisplayHelpInformation();

            _gitRepositoryProcessor = new StubGitRepositoryProcessor();

            _commandLineOptionsRunner = new CommandLineOptionsRunner(_displayHelpInformation, new StubProcessInfo(), _writer, _gitRepositoryProcessor);
        }

        [Test]
        public async Task ShouldDisplayHelpInformation()
        {
            await _commandLineOptionsRunner.RunAsync(new CommandLineOptions()
            {
                Help = true
            }, CancellationToken.None);

            Assert.That(_displayHelpInformation.WasCalled, Is.True);
            Assert.That(_gitRepositoryProcessor.WasCalled, Is.False);
        }

        [Test]
        public async Task ShouldDisplayVersionNumber()
        {
            await _commandLineOptionsRunner.RunAsync(new CommandLineOptions()
            {
                Version = true
            }, CancellationToken.None);

            Assert.That(_writer.Lines, Has.Count.EqualTo(1));
            Assert.That(_writer.Lines[0], Is.EqualTo("ProcessVersion"));
            Assert.That(_gitRepositoryProcessor.WasCalled, Is.False);
        }

        [Test]
        public async Task ShouldStartProcessing()
        {
            await _commandLineOptionsRunner.RunAsync(new CommandLineOptions(), CancellationToken.None);

            Assert.That(_gitRepositoryProcessor.WasCalled, Is.True);
        }
    }
}
