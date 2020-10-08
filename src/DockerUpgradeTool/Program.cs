using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerUpgradeTool.CommandLine;
using DockerUpgradeTool.Files;
using DockerUpgradeTool.Git;
using DockerUpgradeTool.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using PowerArgs;

namespace DockerUpgradeTool
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            try
            {
                var parsed = Args.Parse<CommandLineOptions>(args);

                return Execute(parsed);
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();

                DisplayHelpInformation();

                return Task.CompletedTask;
            }
        }

        private static async Task Execute(CommandLineOptions options)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) => { cts.Cancel(); };

            if (options.Help)
            {
                DisplayHelpInformation();
                return;
            }

            if(options.Version)
            {
                Console.WriteLine(typeof(Program).Assembly.GetName().Version!.ToString());

                return;
            }

            var services = CreateServices(options, cts.Token).BuildServiceProvider();

            await services.GetRequiredService<IGitRepositoryProcessor>().ProcessAsync();
        }

        private static void DisplayHelpInformation()
        {
            var definition = Args.GetAmbientDefinition();

            Console.WriteLine($"Usage: {definition.ExeName} [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Automatically update docker images in github repositories.");
            Console.WriteLine();
            Console.WriteLine("Options:");

            var formattedArguments = definition.Arguments.Select(x => new
            {
                aliases = string.Join('/', x.Aliases.Select(y => $"-{y}")) + (x.IsRequired ? "*" : ""),
                description = x.Description
            });

            var maxLength = formattedArguments.Select(x => x.aliases.Length).Max() + 3;

            foreach (var argument in formattedArguments)
            {
                Console.WriteLine(argument.aliases.PadRight(maxLength) + argument.description);
            }

            Environment.Exit(1);
        }

        public static IServiceCollection CreateServices(CommandLineOptions options, CancellationToken cancellationToken)
        {
            return new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<HttpClient>()
                .AddSingleton<IVersionCache, VersionCache>()
                .AddSingleton(options)
                .AddSingleton<IFileProvider, PhysicalFileProvider>()
                .AddSingleton<ICancellationProvider>(new CancellationProvider(cancellationToken))
                .AddSingleton<IGitHubClient>(sp => {
                    var token = sp.GetRequiredService<IConfigurationOptions>().Token;

                    return new GitHubClient(
                        new ProductHeaderValue("docker-upgrade-tool", "1.0.0"),
                        token == null
                        ? new InMemoryCredentialStore(Credentials.Anonymous)
                        : new InMemoryCredentialStore(new Credentials(token, AuthenticationType.Bearer)));
                })
                .AddSingleton<IGitRepositoryFactory, GitRepositoryFactory>()
                .AddSingleton<IReplacementPlanner, ReplacementPlanner>()
                .AddSingleton<IReplacementPlanExecutor, ReplacementPlanExecutor>()
                .AddSingleton<IFileFilterFactory, FileFilterFactory>()
                .AddSingleton<IGitRepositoryProcessor, GitRepositoryProcessor>()
                .AddSingleton<IConfigurationOptions>(sp => sp.GetRequiredService<IOptions<ConfigurationOptions>>().Value)
                .ConfigureOptions<ConfigureCommandLineOptions>()
                .AddOptions<ConfigurationOptions>()
                .Services;
        }
    }
}
