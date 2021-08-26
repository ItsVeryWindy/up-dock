using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class VersionSearchNode : ParentSearchNode
    {
        private static readonly HashSet<char> ValidCharacters;

        static VersionSearchNode()
        {
            ValidCharacters = new HashSet<char>();

            for(var i = 'a'; i <= 'z'; i++)
            {
                ValidCharacters.Add(i);
            }

            for(var i = 'A'; i <= 'Z'; i++)
            {
                ValidCharacters.Add(i);
            }

            for(var i = '0'; i <= '9'; i++)
            {
                ValidCharacters.Add(i);
            }

            ValidCharacters.Add('.');
            ValidCharacters.Add('-');
        }

        public VersionSearchNode(IEnumerable<ISearchTreeNode> children) : base(children)
        {
        }

        public override SearchTreeNodeResult Search(SearchTreeNodeContext context)
        {
            var versionEndIndex = GetEndOfVersion(context.Span);

            for (var i = 0; i < versionEndIndex; i++)
            {
                var subString = context.Span.Slice(0, versionEndIndex - i).ToString();

                if (!NuGetVersion.TryParse(subString, out var version))
                    continue;

                var childResult = base.Search(context.Next(subString.Length).WithVersion(version));

                if(childResult.Pattern == null)
                    continue;

                return childResult;
            }

            return new SearchTreeNodeResult();
        }

        private static int GetEndOfVersion(ReadOnlySpan<char> span)
        {
            for (var i = 0; i < span.Length; i++)
            {
                var chr = span[i];

                if (!ValidCharacters.Contains(chr))
                {
                    return i;
                }
            }

            return span.Length;
        }
    }
}
