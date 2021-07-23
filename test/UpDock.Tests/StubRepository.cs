using System;

namespace UpDock.Tests
{
    internal class StubRepository : IRepository
    {
        public string FullName => "FullName";

        public string CloneUrl => "CloneUrl";

        public DateTimeOffset? PushedAt => DateTimeOffset.MinValue;

        public string Name => "Name";

        public string Owner => "Owner";

        public string DefaultBranch => "DefaultBranch";
    }
}
