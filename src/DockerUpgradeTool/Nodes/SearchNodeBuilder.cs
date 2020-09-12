using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DockerUpgradeTool.Imaging;
using DockerUpgradeTool.Imaging.Parts;

namespace DockerUpgradeTool.Nodes
{
    public class SearchNodeBuilder
    {
        private readonly TreeNode _root = new TreeNode(TreeNodeType.Root);

        public SearchNodeBuilder Add(DockerImageTemplatePattern pattern)
        {
            _root.Add(pattern);

            return this;
        }

        public ISearchTreeNode Build()
        {
            return _root.Build(ImmutableList<TreeNode>.Empty);
        }

        private enum TreeNodeType
        {
            End,
            Character,
            VersionRange,
            Root
        }

        private class TreeNode
        {
            public char? Character { get; }

            public DockerImageTemplatePattern? Pattern { get; }

            public TreeNodeType Type { get; }

            public List<TreeNode> Children { get; } = new List<TreeNode>();

            public TreeNode(char character)
            {
                Character = character;
                Type = TreeNodeType.Character;
            }

            public TreeNode(DockerImageTemplatePattern pattern)
            {
                Pattern = pattern;
                Type = TreeNodeType.End;
            }

            public TreeNode(TreeNodeType type)
            {
                Type = type;
            }

            public void Add(DockerImageTemplatePattern pattern) => Add(pattern, pattern.Part);

            private void Add(DockerImageTemplatePattern pattern, IDockerImagePatternPart? part)
            {
                if (part == EmptyDockerImagePatternPart.Instance)
                {
                    if(!Children.Any(x => x.Type == TreeNodeType.End && x.Pattern == pattern))
                    {
                        Children.Add(new TreeNode(pattern));
                    }

                    return;
                }

                if (part is VersionDockerImagePatternPart)
                {
                    var versionRangeNode = Children.FirstOrDefault(x => x.Type == TreeNodeType.VersionRange);

                    if (versionRangeNode == null)
                    {
                        versionRangeNode = new TreeNode(TreeNodeType.VersionRange);

                        Children.Add(versionRangeNode);
                    }

                    versionRangeNode.Add(pattern, part.Next);

                    return;
                }

                if (part is TextDockerImagePatternPart textPart)
                {
                    Add(pattern, textPart, textPart.Text.AsSpan());

                    return;
                }

                throw new InvalidOperationException($"Part type {(part is null ? "null" : part.GetType().ToString())} is not supported");
            }

            private void Add(DockerImageTemplatePattern pattern, IDockerImagePatternPart? part, ReadOnlySpan<char> span)
            {
                if (span.IsEmpty)
                {
                    Add(pattern, part?.Next);
                    return;
                }

                var chr = span[0];

                var charNode = Children.FirstOrDefault(x => x.Type == TreeNodeType.Character && x.Character == chr);

                if (charNode == null)
                {
                    charNode = new TreeNode(chr);

                    Children.Add(charNode);
                }

                charNode.Add(pattern, part, span.Slice(1));
            }

            public ISearchTreeNode Build(ImmutableList<TreeNode> nodes)
            {
                switch (Type)
                {
                    case TreeNodeType.Root:
                        return new MultipleSearchNode(Children.Select(x => x.Build(nodes)));
                    case TreeNodeType.End:
                        return new DockerImageTemplatePatternNode(Pattern!);
                }

                var nodesWithThis = nodes.Add(this);

                if (Children.Count == 1)
                {
                    if (Children.Any(x => x.Type == TreeNodeType.End))
                    {
                        return CreateNode(nodesWithThis);
                    }

                    if (Type != TreeNodeType.VersionRange)
                    {
                        var node = Children.First().Build(nodesWithThis);

                        if (node is TextSearchNode)
                        {
                            return node;
                        }
                    }
                }

                return CreateNode(nodesWithThis);
            }

            private ISearchTreeNode CreateNode(ImmutableList<TreeNode> nodes)
            {
                if (Type == TreeNodeType.VersionRange)
                {
                    return new VersionSearchNode(BuildChildren());
                }

                return CreateTextNode(nodes);
            }

            private TextSearchNode CreateTextNode(ImmutableList<TreeNode> nodes)
            {
                var str = new string(nodes.Select(x => x.Character!.Value).ToArray());

                return new TextSearchNode(str, BuildChildren());
            }

            private IEnumerable<ISearchTreeNode> BuildChildren()
            {
                return Children.Select(x => x.Build(ImmutableList<TreeNode>.Empty)).OrderBy(x => x);
            }
        }
    }
}
