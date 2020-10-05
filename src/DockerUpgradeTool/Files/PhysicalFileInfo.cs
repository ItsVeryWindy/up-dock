using System.IO;

namespace DockerUpgradeTool.Files
{
    public class PhysicalFileInfo : IFileInfo
    {
        private readonly IDirectoryInfo? _root;
        private readonly FileInfo _file;

        public IDirectoryInfo? Parent => new PhysicalDirectoryInfo(_file.Directory);
        public string Name => _file.Name;
        public string Path => _file.FullName;
        public bool Exists => _file.Exists;

        public IDirectoryInfo? Root => _root;

        public PhysicalFileInfo(IDirectoryInfo? root, FileInfo file)
        {
            _root = root;
            _file = file;
        }

        public void Delete() => _file.Delete();

        public Stream CreateWriteStream() => new FileStream(Path, FileMode.Truncate, FileAccess.Write);

        public Stream CreateReadStream() => new FileStream(Path, FileMode.Open, FileAccess.Read);

        public void Move(IFileInfo file) => File.Move(Path, file.Path);

        public string MakeRelativePath(IDirectoryInfo directory) => System.IO.Path.GetRelativePath(directory.Path, _file.FullName);
    }
}
