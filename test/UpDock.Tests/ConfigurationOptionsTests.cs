using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UpDock.Imaging;
using NUnit.Framework;

namespace UpDock.Tests
{
    public class ConfigurationOptionsTests
    {
        [Test]
        public async Task ShouldParseConfigurationFile()
        {
            var ms = new MemoryStream();

            await JsonSerializer.SerializeAsync(ms, new
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

            ms.Position = 0;

            var options = new ConfigurationOptions();

            options.Populate(ms);

            Assert.That(options.Include, Has.Count.EqualTo(1));
            Assert.That(options.Include, Does.Contain("include"));
            Assert.That(options.Exclude, Has.Count.EqualTo(1));
            Assert.That(options.Exclude, Does.Contain("exclude"));
            Assert.That(options.Patterns, Has.Count.EqualTo(3));

            var first = options.Patterns.First();

            Assert.That(first.ToString(), Is.EqualTo("example-image:{v}"));
            Assert.That(first.Template.Image, Is.EqualTo("library/example-image"));
            Assert.That(first.Template.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(first.Template.Tag, Is.EqualTo("{v*}"));

            var second = options.Patterns.Skip(1).First();

            Assert.That(second.ToString(), Is.EqualTo("example-image:{v}"));
            Assert.That(second.Template.Image, Is.EqualTo("example-image"));
            Assert.That(second.Template.Repository.ToString(), Is.EqualTo("https://example-repository/"));
            Assert.That(second.Template.Tag, Is.EqualTo("{v*}"));

            var third = options.Patterns.Skip(2).First();

            Assert.That(third.ToString(), Is.EqualTo("example-pattern"));
            Assert.That(third.Template.Image, Is.EqualTo("example-image"));
            Assert.That(third.Template.Repository?.ToString(), Is.EqualTo("https://example-repository/"));
            Assert.That(third.Template.Tag, Is.EqualTo("example-tag"));
        }
    }
}
