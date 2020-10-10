using DockerUpgradeTool.CommandLine;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests.CommandLine
{
    public class DisplayHelpInformationTests
    {

#pragma warning disable CS8618
        private StubConsoleWriter _writer;
        private DisplayHelpInformation _displayHelpInformation;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _writer = new StubConsoleWriter();

            _displayHelpInformation = new DisplayHelpInformation(_writer, new StubProcessInfo());
        }

        [Test]
        public void ShouldPrintHelpInformation()
        {
            _displayHelpInformation.Display<CommandLineOptions>();

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] {
                "Usage: ProcessName [OPTIONS]",
                null,
                "Automatically update docker images in github repositories.",
                null,
                "Options:",
                "--auth/-a        Authentication for a repository",
                "--config/-c      Default configuration to apply",
                "--dry-run/-d     Run without creating pull requests",
                "--email/-e*      Email to use in the commit",
                "--help/-h        Display help information",
                "--search/-s*     Search query to get repositories",
                "--template/-i    A template to apply",
                "--token/-t       GitHub token to access the repository",                
                "--version/-v     Display what the version is"
            }));
        }
    }
}
