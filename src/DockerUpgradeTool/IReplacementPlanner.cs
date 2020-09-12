using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.Files;
using DockerUpgradeTool.Nodes;

namespace DockerUpgradeTool
{
    public interface IReplacementPlanner
    {
        Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IFileInfo file, ISearchTreeNode node, CancellationToken cancellationToken);
    }
}
