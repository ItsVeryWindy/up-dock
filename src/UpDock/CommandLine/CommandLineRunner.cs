using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UpDock.CommandLine
{
    public static class CommandLineRunner
    {
        public static async Task RunAsync<TCommandLineOptions, TRunner>(Action<IServiceCollection> configureServices, Action<ILoggingBuilder> configureLogging, string[] args)
            where TCommandLineOptions : class, new()
            where TRunner : ICommandLineRunner<TCommandLineOptions>
        {
            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) => { cts.Cancel(); };

            var services = new ServiceCollection();

            services
                .AddLogging(configureLogging)
                .AddSingleton<IConsoleWriter, ConsoleWriter>()
                .AddSingleton<ICommandLineBinder, CommandLineBinder>()
                .AddSingleton<ICommandLineParser, CommandLineParser>()
                .AddSingleton<ICommandLineValidator, CommandLineValidator>()
                .AddSingleton<IDisplayErrorMessages, DisplayErrorMessages>()
                .AddSingleton<IDisplayHelpInformation, DisplayHelpInformation>()
                .AddSingleton<IProcessInfo>(new ProcessInfo(
                    Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName),
                    typeof(CommandLineRunner).Assembly.GetName().Version!.ToString()
                ));

            configureServices(services);

            var options = new TCommandLineOptions();

            services.AddSingleton(options);

            var serviceProvider = services.BuildServiceProvider();

            var parser = serviceProvider.GetRequiredService<ICommandLineParser>();

            var arguments = parser.Parse<TCommandLineOptions>(args);

            serviceProvider.GetRequiredService<ICommandLineValidator>().Validate<TCommandLineOptions>(arguments);

            if(arguments.Any(x => x.Errors.Any()))
            {
                serviceProvider.GetRequiredService<IDisplayErrorMessages>().Display(arguments);
                serviceProvider.GetRequiredService<IDisplayHelpInformation>().Display<TCommandLineOptions>();

                Environment.Exit(1);
            }

            serviceProvider.GetRequiredService<ICommandLineBinder>().Bind(arguments, options);

            var runner = ActivatorUtilities.CreateInstance<TRunner>(serviceProvider);

            try
            {
                await runner.RunAsync(options, cts.Token);
            }
            finally
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }
}
