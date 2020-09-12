using System.Collections.Generic;
using System.Linq;
using DockerUpgradeTool.Git;
using DotNet.Globbing;

namespace DockerUpgradeTool.Files
{
    public class FileFilter : IFileFilter
    {
        private readonly List<Glob> _include;
        private readonly List<Glob> _exclude;

        public FileFilter(IConfigurationOptions options)
        {
            _include = options.Include.Select(Glob.Parse).ToList();
            _exclude = options.Exclude.Select(Glob.Parse).ToList();
        }

        public bool Filter(ILocalGitRepository repository, IFileInfo file)
        {
            var relativePath = file.MakeRelativePath(repository.Directory);

            if (repository.Ignored(file))
                return false;

            if(_include.Count > 0 && !_include.Any(x => x.IsMatch(relativePath)))
                return false;

            if (_exclude.Any(x => x.IsMatch(relativePath)))
                return false;

            return !InGitDirectory(repository.Directory, file);
        }

        private static bool InGitDirectory(IDirectoryInfo directory, IFileInfo file)
        {
            var parent = file.Parent;

            while(parent != null && parent.Path != directory.Path)
            {
                if (parent.Name == ".git")
                    return true;

                parent = parent.Parent;
            }

            return false;
        }
    }
}
