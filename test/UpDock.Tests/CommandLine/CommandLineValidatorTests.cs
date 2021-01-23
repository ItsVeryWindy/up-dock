using System;
using System.Collections.Generic;
using System.Linq;
using UpDock.CommandLine;
using UpDock.Files;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace UpDock.Tests.CommandLine
{
    public class CommandLineValidatorTests
    {
#pragma warning disable CS8618
        private StubFileProvider _fileProvider;
        private CommandLineValidator _commandLineValidator;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _fileProvider = new StubFileProvider();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFileProvider>(_fileProvider)
                .BuildServiceProvider();

            _commandLineValidator = new CommandLineValidator(serviceProvider);
        }

        [TestCase(null, "The --email field is required.")]
        [TestCase("hello", "The --email field is not a valid e-mail address.")]
        public void ShouldHandleInvalidEmail(string value, string error)
        {
            var argument = new CommandLineArgument("--email", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Email)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo(error));
        }

        [TestCase("test@test.com")]
        public void ShouldHandleValidEmail(string value)
        {
            var argument = new CommandLineArgument("--email", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Email)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Is.Empty);
        }

        [TestCase(null, "The --search field is required.")]
        [TestCase("topic:hello", "The --search field should contain one of org, repo or user search options.")]
        public void ShouldHandleInvalidSearch(string value, string error)
        {
            var argument = new CommandLineArgument("--search", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Search)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo(error));
        }

        [TestCase("org:MyOrganisation")]
        [TestCase("repo:My/Repo")]
        [TestCase("user:MyUser")]
        [TestCase("user:MyUser other search terms")]
        public void ShouldHandleValidSearch(string value)
        {
            var argument = new CommandLineArgument("--search", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Search)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Is.Empty);
        }

        [TestCase("not-valid", "The equals sign is missing.")]
        [TestCase("not-valid=", "The separator is missing.")]
        [TestCase("not-valid=a", "The separator is missing.")]
        public void ShouldHandleInvalidAuthentication(string value, string error)
        {
            var emailArgument = new CommandLineArgument("--auth", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Authentication)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                emailArgument
            });

            Assert.That(emailArgument.Errors, Has.Count.EqualTo(1));
            Assert.That(emailArgument.Errors.First(), Is.EqualTo(error));
        }

        [TestCase("valid.com=a,b")]
        public void ShouldHandleValidAuthentication(string value)
        {
            var argument = new CommandLineArgument("--auth", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Authentication)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Is.Empty);
        }

        [TestCase("invalid/path", "", "Could not find the file specified in the --config field.")]
        [TestCase("my/file", "not-valid-json", "The file specified in the --config field is not valid json.")]
        public void ShouldHandleInvalidConfig(string value, string contents, string error)
        {
            const string path = "my/file";

            _fileProvider.AddFile(path, contents);

            var argument = new CommandLineArgument("--config", null, value, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Config)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Has.Count.EqualTo(1));
            Assert.That(argument.Errors.First(), Is.EqualTo(error));
        }

        [Test]
        public void ShouldHandleValidConfig()
        {
            const string path = "my/file";

            _fileProvider.AddFile(path, "{}");

            var argument = new CommandLineArgument("--config", null, path, typeof(CommandLineOptions).GetProperty(nameof(CommandLineOptions.Config)), 0);

            _commandLineValidator.Validate<CommandLineOptions>(new List<CommandLineArgument>
            {
                argument
            });

            Assert.That(argument.Errors, Is.Empty);
        }
    }
}
