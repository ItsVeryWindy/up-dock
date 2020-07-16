using DockerUpgrader.Imaging;
using NUnit.Framework;

namespace DockerUpgrader.Tests
{
    public class DockerImageTemplateTests
    {
        [TestCase("nginx", "https://registry-1.docker.io/", "library/nginx", "nginx:{v}")]
        [TestCase("library/nginx", "https://registry-1.docker.io/", "library/nginx", "library/nginx:{v}")]
        [TestCase("repository.com/nginx", "https://repository.com/", "nginx", "repository.com/nginx:{v}")]
        public void ParseTests(string str, string expectedRepository, string expectedImage, string expectedPattern)
        {
            var template = DockerImageTemplate.ParseTemplate(str);

            Assert.That(template.Repository.ToString(), Is.EqualTo(expectedRepository));
            Assert.That(template.Image, Is.EqualTo(expectedImage));

            var pattern = template.CreatePattern(true, true);

            Assert.That(pattern.ToString(), Is.EqualTo(expectedPattern));
        }
    }
}
