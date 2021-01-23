using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpDock.Git;
using UpDock.Nodes;

namespace UpDock
{
    public interface IReplacementPlanner
    {
        Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IRepositoryFileInfo file, ISearchTreeNode node, CancellationToken cancellationToken);
    }
}
