using System;
using System.Diagnostics;

namespace UpDock.Git.Drivers
{
    public class GitProcessException : Exception
    {
        public GitProcessException(Process process) : base($"Git command failed to execute: {string.Join(' ', process.StartInfo.Arguments)}")
        {

        }
    }
}
