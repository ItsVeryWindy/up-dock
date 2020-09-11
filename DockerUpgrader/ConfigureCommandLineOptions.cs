using System;
using DockerUpgrader.Files;
using Microsoft.Extensions.Options;

namespace DockerUpgrader
{
    public class ConfigureCommandLineOptions : IConfigureOptions<ConfigurationOptions>
    {
        private readonly CommandLineOptions _options;
        private readonly IFileProvider _provider;

        public ConfigureCommandLineOptions(CommandLineOptions options, IFileProvider provider)
        {
            _options = options;
            _provider = provider;
        }

        public void Configure(ConfigurationOptions options)
        {
            options.Search ??= _options.Search;
            options.Token ??= _options.Token;

            if (_options.Templates != null)
            {
                foreach (var template in _options.Templates)
                {
                    options.Patterns.Add(ConfigurationOptions.ParsePattern(template));
                }
            }

            if (_options.Config != null)
            {
                var stream = _provider.GetFile(_options.Config)?.CreateReadStream();

                if (stream != null)
                {
                    options.Populate(stream);
                }
            }

            PopulateAuthentication(options);            
        }

        private void PopulateAuthentication(ConfigurationOptions options)
        {
            if(_options.Authentication == null)
                return;

            foreach (var authentication in _options.Authentication)
            {
                var result = ParseAuthentication(authentication);

                options.Authentication[result.repo] = result.options;
            }
        }

        private (string repo, AuthenticationOptions options) ParseAuthentication(string authentication)
        {
            var repoSplit = authentication.Split("=", 2);

            if (repoSplit.Length < 2)
                throw new FormatException("Missing equals in authentication");

            var (repo, str) = (repoSplit[0], repoSplit[1]);

            var strSplit = str.Split(',', 2);

            if (repoSplit.Length < 2)
                throw new FormatException("Missing authentication separator");

            return (repo, new AuthenticationOptions(strSplit[0], strSplit[1]));
        }
    }
}