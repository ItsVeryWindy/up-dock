using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock
{
    public interface IReplacementPlanExecutor
    {
        Task ExecutePlanAsync(IReadOnlyCollection<TextReplacement> replacements, CancellationToken cancellationToken);
    }
}
