using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.Git;
using DockerUpgradeTool.Nodes;

namespace DockerUpgradeTool
{
    public interface IReplacementPlanner
    {
        Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IRepositoryFileInfo file, ISearchTreeNode node, CancellationToken cancellationToken);
    }
}
