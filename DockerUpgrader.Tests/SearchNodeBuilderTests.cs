using DockerUpgrader.Imaging;
using DockerUpgrader.Nodes;
using NUnit.Framework;

namespace DockerUpgrader.Tests
{
    public class SearchNodeBuilderTests
    {
        [Test]
        public void ShouldSearchWithOneItem()
        {
            var builder = new SearchNodeBuilder();

            var template = DockerImageTemplate.ParseTemplate("abcd1234:{v}");

            builder.Add(template.CreatePattern(true, true));

            var node = builder.Build();

            var result = node.Search("abcd1234:1.2.3").Pattern;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Image, Is.Not.Null);
            Assert.That(result.Image.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(result.Image.Image, Is.EqualTo("library/abcd1234"));
            Assert.That(result.Image.Tag, Is.EqualTo("1.2.3"));
            Assert.That(result.Image.Template, Is.EqualTo(template));
        }

        [Test]
        public void ShouldSearchWithTwoItems()
        {
            var builder = new SearchNodeBuilder();

            var template = DockerImageTemplate.ParseTemplate("abcd12345:{v}");

            builder.Add(DockerImageTemplate.ParseTemplate("abcd1234:{v}").CreatePattern(true, true));
            builder.Add(template.CreatePattern(true, true));

            var node = builder.Build();

            var result = node.Search("abcd12345:1.2.3").Pattern;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Image, Is.Not.Null);
            Assert.That(result.Image.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(result.Image.Image, Is.EqualTo("library/abcd12345"));
            Assert.That(result.Image.Tag, Is.EqualTo("1.2.3"));
            Assert.That(result.Image.Template, Is.EqualTo(template));
        }

        [TestCase("abcd1234:1.2.3", 14)]
        [TestCase("abcd1234:1.2.3+abcd", 14)]
        public void ShouldSearchWithOneItemWithVersionLast(string search, int expectedEndIndex)
        {
            var builder = new SearchNodeBuilder();

            var template = DockerImageTemplate.ParseTemplate("abcd1234:{v}");
                
            var pattern = template.CreatePattern(true, true);

            builder.Add(pattern);

            var node = builder.Build();

            var result = node.Search(search);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Pattern, Is.Not.Null);
            Assert.That(result.Pattern!.Image, Is.Not.Null);
            Assert.That(result.Pattern.Image.Repository, Is.EqualTo(DockerImageTemplate.DefaultRepository));
            Assert.That(result.Pattern.Image.Image, Is.EqualTo("library/abcd1234"));
            Assert.That(result.Pattern.Image.Tag, Is.EqualTo("1.2.3"));
            Assert.That(result.Pattern.Image.Template, Is.EqualTo(template));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }
    }
}
