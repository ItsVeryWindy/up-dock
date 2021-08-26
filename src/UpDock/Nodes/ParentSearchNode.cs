using System.Collections.Generic;

namespace UpDock.Nodes
{
    public class ParentSearchNode : ISearchTreeNode
    {
        private readonly List<ISearchTreeNode> _children;

        public ParentSearchNode(IEnumerable<ISearchTreeNode> children)
        {
            _children = new List<ISearchTreeNode>(children);
        }

        public virtual SearchTreeNodeResult Search(SearchTreeNodeContext context)
        {
            foreach (var child in _children)
            {
                var childResult = child.Search(context);

                if (childResult.Pattern != null)
                    return childResult;
            }

            return new SearchTreeNodeResult();
        }

        public virtual int CompareTo(ISearchTreeNode? other) => -1;
    }
}
