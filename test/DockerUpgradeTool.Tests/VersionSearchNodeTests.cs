using System.Linq;
using DockerUpgradeTool.Nodes;
using NUnit.Framework;

namespace DockerUpgradeTool.Tests
{
    public class VersionSearchNodeTests
    {
        [TestCase("2.0.0alpine", 11)]
        [TestCase("2.0.0-alpinealpine", 18)]
        [TestCase("2.0.0-rc.2alpine", 16)]
        [TestCase("2.0.0-rc.1alpine", 16)]
        [TestCase("1.0.0alpine", 11)]
        [TestCase("1.0.0-betaalpine", 16)]
        [TestCase("1.0.0-betaalpine+superfluous", 16)]
        public void ShouldReturnResultWithVersion(string str, int expectedEndIndex)
        {
            var textSearchNode = new TextSearchNode("alpine", StubSearchNode.SingleInstance);

            var node = new VersionSearchNode(Enumerable.Repeat<ISearchTreeNode>(textSearchNode, 1));

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }

        [TestCase("2.0.0", 5)]
        [TestCase("2.0.0-rc.2", 10)]
        [TestCase("2.0.0-rc.1", 10)]
        [TestCase("1.0.0", 5)]
        [TestCase("1.0.0-beta", 10)]
        [TestCase("1.0.0-beta+superfluous", 10)]
        public void ShouldReturnResultWithVersionLast(string str, int expectedEndIndex)
        {
            var node = new VersionSearchNode(StubSearchNode.SingleInstance);

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }
    }
}
