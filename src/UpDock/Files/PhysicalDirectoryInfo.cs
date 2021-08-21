using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UpDock.Files
{
    public class PhysicalDirectoryInfo : IDirectoryInfo
    {
        private readonly DirectoryInfo _directory;

        public PhysicalDirectoryInfo(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public IEnumerable<IFileInfo> AllFiles => _directory.GetFiles("*", SearchOption.AllDirectories).Select(x => new PhysicalFileInfo(x));

        public string AbsolutePath => _directory.FullName;

        public string Name => _directory.Name;

        public IDirectoryInfo? Parent => _directory.Parent == null ? null : new PhysicalDirectoryInfo(_directory.Parent);

        public bool Exists => _directory.Exists;

        public IEnumerable<IFileInfo> Files => _directory.GetFiles().Select(x => new PhysicalFileInfo(x));

        public IEnumerable<IDirectoryInfo> Directories => _directory.GetDirectories().Select(x => new PhysicalDirectoryInfo(x));

        public IDirectoryInfo Create()
        {
            Directory.CreateDirectory(AbsolutePath);

            return this;
        }

        public void Delete() => _directory.Delete(true);

        public IFileInfo GetFile(string relativePath) => new PhysicalFileInfo(new FileInfo(Path.Join(_directory.FullName, relativePath)));

        public IDirectoryInfo SetAttributes(FileAttributes fileAttributes)
        {
            File.SetAttributes(_directory.FullName, fileAttributes);

            return this;
        }
    }
}
