using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using UpDock.Files;

namespace UpDock.Git.Drivers
{
    public class LibGit2SharpDriver : IGitDriver
    {
        private readonly IFileProvider _provider;

        public LibGit2SharpDriver(IFileProvider provider)
        {
            _provider = provider;
        }

        public IRepository Clone(string cloneUrl, string dir, string? token)
        {
            UsernamePasswordCredentials CreateCredentials(string url, string user, SupportedCredentialTypes cred)
            {
                return new UsernamePasswordCredentials
                {
                    Username = "username", Password = token
                };
            }

            var co = new CloneOptions
            {
                CredentialsProvider = CreateCredentials
            };

            var path = Repository.Clone(cloneUrl, dir, co);

            return new LibGit2SharpRepository(new Repository(path), _provider, CreateCredentials);
        }
    }
}
