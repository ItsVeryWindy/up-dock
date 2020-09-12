using System.Linq;
using DockerUpgradeTool.Nodes;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests
{
    public class VersionSearchNodeTests
    {
        [TestCase("2.0.0alpine", "2.0.0", 11)]
        [TestCase("2.0.0-alpinealpine", "2.0.0-alpine", 18)]
        [TestCase("2.0.0-rc.2alpine", "2.0.0-rc.2", 16)]
        [TestCase("2.0.0-rc.1alpine", "2.0.0-rc.1", 16)]
        [TestCase("1.0.0alpine", "1.0.0", 11)]
        [TestCase("1.0.0-betaalpine","1.0.0-beta", 16)]
        [TestCase("1.0.0-betaalpine+superfluous", "1.0.0-beta", 16)]
        public void ShouldReturnResultWithVersion(string str, string expectedVersion, int expectedEndIndex)
        {
            var textSearchNode = new TextSearchNode("alpine", StubSearchNode.SingleInstance);

            var node = new VersionSearchNode(Enumerable.Repeat<ISearchTreeNode>(textSearchNode, 1));

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }

        [TestCase("2.0.0", "2.0.0", 5)]
        [TestCase("2.0.0-rc.2", "2.0.0-rc.2", 10)]
        [TestCase("2.0.0-rc.1", "2.0.0-rc.1", 10)]
        [TestCase("1.0.0", "1.0.0", 5)]
        [TestCase("1.0.0-beta", "1.0.0-beta",  10)]
        [TestCase("1.0.0-beta+superfluous", "1.0.0-beta", 10)]
        public void ShouldReturnResultWithVersionLast(string str, string expectedVersion, int expectedEndIndex)
        {
            var node = new VersionSearchNode(StubSearchNode.SingleInstance);

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }
    }
}
