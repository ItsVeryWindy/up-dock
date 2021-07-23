using System;

namespace UpDock
{
    public interface IRepository
    {
        string FullName { get; }
        string CloneUrl { get; }
        DateTimeOffset? PushedAt { get; }
        string Name { get; }
        string Owner { get; }
        string DefaultBranch { get; }
    }
}
