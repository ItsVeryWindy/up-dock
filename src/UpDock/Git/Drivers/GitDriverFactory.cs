using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDock.CommandLine;

namespace UpDock.Git.Drivers
{
    public class GitDriverFactory
    {
        private readonly CommandLineOptions _options;
        private readonly LibGit2SharpDriver _libGit2SharpDriver;
        private readonly GitProcessDriver _gitProcessDriver;

        public GitDriverFactory(CommandLineOptions options, LibGit2SharpDriver libGit2SharpDriver, GitProcessDriver gitProcessDriver)
        {
            _options = options;
            _libGit2SharpDriver = libGit2SharpDriver;
            _gitProcessDriver = gitProcessDriver;
        }

        public IGitDriver Create() => _options.UseGit ? _gitProcessDriver : _libGit2SharpDriver;
    }
}
