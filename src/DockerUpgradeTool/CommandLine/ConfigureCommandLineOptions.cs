using System;
using DockerUpgradeTool.Files;
using Microsoft.Extensions.Options;

namespace DockerUpgradeTool.CommandLine
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
            options.DryRun = _options.DryRun;

            if (_options.Templates != null)
            {
                foreach (var template in _options.Templates)
                {
                    options.Patterns.Add(template);
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
                var result = AuthenticationOptions.Parse(authentication);

                options.Authentication[result.repo] = result.options;
            }
        }
    }
}
