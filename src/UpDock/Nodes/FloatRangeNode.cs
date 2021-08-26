using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuGet.Versioning;

namespace UpDock.Nodes
{
    public class FloatRangeNode : ParentSearchNode
    {
        private readonly FloatRange _range;

        public FloatRangeNode(FloatRange range, IEnumerable<ISearchTreeNode> children) : base(children)
        {
            _range = range;
        }

        public override SearchTreeNodeResult Search(SearchTreeNodeContext context)
        {
            var version = context.Versions.Last();

            if (!_range.Satisfies(version))
                return new SearchTreeNodeResult();

            return base.Search(context);
        }
    }
}
