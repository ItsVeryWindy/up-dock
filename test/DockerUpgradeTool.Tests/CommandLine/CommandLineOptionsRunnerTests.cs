using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.CommandLine;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests.CommandLine
{
    public class CommandLineOptionsRunnerTests
    {
#pragma warning disable CS8618
        private StubConsoleWriter _writer;
        private StubDisplayHelpInformation _displayHelpInformation;
        private StubGitRepositoryProcessor _gitRepositoryProcessor;
        private CommandLineOptionsRunner _commandLineOptionsRunner;
#pragma warning restore CS8618

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
            Assert.That(_writer.Lines.First(), Is.EqualTo("ProcessVersion"));
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
