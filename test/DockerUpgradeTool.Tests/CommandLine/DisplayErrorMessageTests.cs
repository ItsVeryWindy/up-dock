using System.Collections.Generic;
using System.Linq;
using DockerUpgradeTool.CommandLine;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests.CommandLine
{
    public class DisplayErrorMessageTests
    {
#pragma warning disable CS8618
        private StubConsoleWriter _writer;
        private DisplayErrorMessages _displayErrorMessages;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _writer = new StubConsoleWriter();

            _displayErrorMessages = new DisplayErrorMessages(_writer);
        }

        [Test]
        public void ShouldPrintErrorsToTheConsole()
        {
            _displayErrorMessages.Display(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", "value", null, null, 0)
                {
                    Errors =
                    {
                        "This is wrong"
                    }
                }
            });

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] {
                "--argument: (value)",
                "            This is wrong",
                null
            }));
        }

        [Test]
        public void ShouldNotPrintNoErrorsToTheConsole()
        {
            _displayErrorMessages.Display(new List<CommandLineArgument>()
            {
                new CommandLineArgument("--argument", null, null, null, 0)
            });

            Assert.That(_writer.Lines, Is.EquivalentTo(new string?[] { null }));
        }
    }
}
