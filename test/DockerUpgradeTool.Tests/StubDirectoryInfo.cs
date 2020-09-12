using System;
using System.Collections.Generic;
using DockerUpgradeTool.Files;

namespace DockerUpgradeTool.Tests
{
    internal class StubDirectoryInfo : IDirectoryInfo
    {
        public IEnumerable<IFileInfo> Files { get; }
        public string Path { get; }
        public string Name { get; }
        public IDirectoryInfo? Parent { get; }

        public StubDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }
    }
}
