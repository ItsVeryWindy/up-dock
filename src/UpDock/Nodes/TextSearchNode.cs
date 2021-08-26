using System;
using System.Collections.Generic;

namespace UpDock.Nodes
{
    public class TextSearchNode : ParentSearchNode
    {
        private readonly string _text;

        public TextSearchNode(string text, IEnumerable<ISearchTreeNode> children) : base(children)
        {
            _text = text;
        }

        public override SearchTreeNodeResult Search(SearchTreeNodeContext context)
        {
            var result = context.Span.StartsWith(_text, StringComparison.InvariantCultureIgnoreCase);

            return result ? base.Search(context.Next(_text.Length)) : new SearchTreeNodeResult();
        }

        public override int CompareTo(ISearchTreeNode? other)
        {
            if (other is TextSearchNode textSearchNode)
            {
                return _text.Length >= textSearchNode._text.Length ? -1 : 1;
            }

            if (other is VersionSearchNode)
                return 1;

            return -1;
        }
    }
}
