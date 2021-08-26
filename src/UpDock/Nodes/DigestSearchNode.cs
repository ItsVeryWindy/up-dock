using System;
using System.Collections.Generic;

namespace UpDock.Nodes
{
    public class DigestSearchNode : ParentSearchNode
    {
        private const string DigestStart = "sha256:";
        private const int DigestLength = 64;

        public DigestSearchNode(IEnumerable<ISearchTreeNode> children) : base(children)
        {
        }

        public override SearchTreeNodeResult Search(SearchTreeNodeContext context)
        {
            var length = DigestStart.Length + DigestLength;

            var sha = context.Span.Slice(0, length);

            if (!sha.StartsWith(DigestStart))
                return new();

            if (sha.Length < length)
                return new();

            foreach(var chr in sha[DigestStart.Length..])
            {
                if ((chr < 'a' || chr > 'z') && !char.IsDigit(chr))
                    return new();
            }

            return base.Search(context.Next(length).WithDigest(sha));
        }
    }
}
