using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgrader.Files;
using DockerUpgrader.Nodes;

namespace DockerUpgrader
{
    public interface IReplacementPlanner
    {
        Task<IReadOnlyCollection<TextReplacement>> GetReplacementPlanAsync(IFileInfo file, ISearchTreeNode node, CancellationToken cancellationToken);
    }
}