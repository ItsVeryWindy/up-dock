using System;

namespace UpDock.Nodes
{
    public interface ISearchTreeNode : IComparable<ISearchTreeNode>
    {
        SearchTreeNodeResult Search(SearchTreeNodeContext context);
    }
}
