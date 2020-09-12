using System.Linq;
using DockerUpgrader.Nodes;
using NUnit.Framework;

namespace DockerUpgrader.Tests
{
    public class TextSearchNodeTests
    {
        [TestCase("beforealpine", 12)]
        [TestCase("beforealpineplusothertext", 12)]
        public void ShouldFindSearchText(string str, int expectedEndIndex)
        {
            var textSearchNode = new TextSearchNode("alpine", StubSearchNode.SingleInstance);

            var node = new TextSearchNode("before",Enumerable.Repeat<ISearchTreeNode>(textSearchNode, 1));

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }

        [TestCase("alpine", 6)]
        [TestCase("alpinetext-after", 6)]
        public void ShouldReturnAResult(string str, int expectedEndIndex)
        {
            var node = new TextSearchNode("alpine", StubSearchNode.SingleInstance);

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.EqualTo(StubSearchNode.Image));
            Assert.That(result.EndIndex, Is.EqualTo(expectedEndIndex));
        }

        [TestCase("alpin")]
        [TestCase("notalpine")]
        public void ShouldNotReturnAResult(string str)
        {
            var node = new TextSearchNode("alpine", StubSearchNode.SingleInstance);

            var result = node.Search(str);

            Assert.That(result.Pattern, Is.Null);
            Assert.That(result.EndIndex, Is.EqualTo(0));
        }
    }
}
