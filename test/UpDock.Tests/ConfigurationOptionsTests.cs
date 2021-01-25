using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UpDock.Imaging;
using NUnit.Framework;
using System;

namespace UpDock.Tests
{
    public class ConfigurationOptionsTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ConfigurationOptions _options;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [SetUp]
        public void SetUp()
        {
            _options = new ConfigurationOptions();
        }

        [Test]
        public async Task ShouldParseConfigurationFile()
        {
            var ms = await CreateStream(new
            {
                include = "include",
                exclude = "exclude",
                templates = new object[]
                {
                    "example-image",
                    new
                    {
                        image = "example-image",
                        repository = "example-repository"
                    },
                    new
                    {
                        pattern = "example-pattern",
                        image = "example-image:example-tag",
                        repository = "example-repository"
                    }
                }
            });

            _options.Populate(ms);

            Assert.That(_options.Include, Has.Count.EqualTo(1));
            Assert.That(_options.Include, Does.Contain("include"));
            Assert.That(_options.Exclude, Has.Count.EqualTo(1));
            Assert.That(_options.Exclude, Does.Contain("exclude"));
            Assert.That(_options.Patterns, Has.Count.EqualTo(3));

            var first = _options.Patterns.First();

            Assert.That(first.ToString(), Is.EqualTo("example-image:{v}"));
            Assert.That(first.Template.Image, Is.EqualTo("library/example-image"));
            Assert.That(first.Template.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(first.Template.Tag, Is.EqualTo("{v*}"));

            var second = _options.Patterns.Skip(1).First();

            Assert.That(second.ToString(), Is.EqualTo("example-image:{v}"));
            Assert.That(second.Template.Image, Is.EqualTo("example-image"));
            Assert.That(second.Template.Repository.ToString(), Is.EqualTo("https://example-repository/"));
            Assert.That(second.Template.Tag, Is.EqualTo("{v*}"));

            var third = _options.Patterns.Skip(2).First();

            Assert.That(third.ToString(), Is.EqualTo("example-pattern"));
            Assert.That(third.Template.Image, Is.EqualTo("example-image"));
            Assert.That(third.Template.Repository?.ToString(), Is.EqualTo("https://example-repository/"));
            Assert.That(third.Template.Tag, Is.EqualTo("example-tag"));
        }

        [Test]
        public async Task ShouldHandleImageMissing()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        repository = "example-repository"
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected image was not found"));
        }

        [Test]
        public async Task ShouldHandleRepositoryMissing()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        image = "example-image"
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected repository was not found"));
        }

        [Test]
        public async Task ShouldHandleRepositoryNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        repository = Array.Empty<object>(),
                        image = "example-image"
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected repository to be a string"));
        }

        [Test]
        public async Task ShouldHandleImageNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        repository = "example-repository",
                        image = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected image to be a string"));
        }

        [Test]
        public async Task ShouldHandleGroupNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        repository = "example-repository",
                        image = "example-image",
                        group = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected group to be a string"));
        }

        [Test]
        public async Task ShouldHandlePatternNotBeingAString()
        {
            var ms = await CreateStream(new {
                templates = new object[]
                {
                    new
                    {
                        repository = "example-repository",
                        image = "example-image",
                        pattern = Array.Empty<object>()
                    }
                }
            });

            Assert.That(() => _options.Populate(ms), Throws.InvalidOperationException.And.Message.EqualTo("Invalid configuration file: expected pattern to be a string"));
        }

        private static async Task<MemoryStream> CreateStream(object obj)
        {
            var ms = new MemoryStream();

            await JsonSerializer.SerializeAsync(ms, obj);

            ms.Position = 0;

            return ms;
        }
    }
}
