using System.Collections.Generic;
using System.Linq;
using UpDock.Git;
using DotNet.Globbing;
using System.Threading;
using System.Threading.Tasks;

namespace UpDock.Files
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

        public async Task<bool> FilterAsync(IRepositoryFileInfo file, CancellationToken cancellationToken)
        {
            var relativePath = file.RelativePath;

            var ignored = await file.IsIgnoredAsync(cancellationToken);

            if (ignored)
                return false;

            if(_include.Count > 0 && !_include.Any(x => x.IsMatch(relativePath)))
                return false;

            if (_exclude.Any(x => x.IsMatch(relativePath)))
                return false;

            return !InGitDirectory(file.Root, file.File);
        }

        private static bool InGitDirectory(IDirectoryInfo directory, IFileInfo file)
        {
            var parent = file.Parent;

            while(parent != null && parent.AbsolutePath != directory.AbsolutePath)
            {
                if (parent.Name == ".git")
                    return true;

                parent = parent.Parent;
            }

            return false;
        }
    }
}
