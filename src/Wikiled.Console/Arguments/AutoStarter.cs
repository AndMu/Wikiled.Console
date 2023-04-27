using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly Dictionary<string, (ICommandConfig Config, ServiceDescriptor Service)> configs = new(StringComparer.OrdinalIgnoreCase);

        public AutoStarter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            using var loggerFactory = LoggerFactory.Create(
                loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace).AddConsole());
            Logger = loggerFactory.CreateLogger("Main");
            Name = name;
        }

        public string Name { get; }

        public ILogger Logger { get; }

        public AppConfig Config { get; } = new ();

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : ICommand
            where TConfig : ICommandConfig, new()
        {
            var descriptor = new ServiceDescriptor(typeof(ICommand), typeof(T), ServiceLifetime.Singleton);
            configs[name] = (new TConfig(), descriptor);
            return this;
        }

        public IHostBuilder Build(string[] args)
        {
            Logger.LogInformation("Starting {0} version {1}...", Assembly.GetEntryAssembly()?.GetName().Version, Name);
            if (args.Length == 0)
            {
                Logger.LogError("Please specify arguments");
                throw new Exception("Please specify arguments");
            }

            if (args.Length == 0)
            {
                Logger.LogError("Please specify command");
                throw new Exception("Please specify command");
            }

            var name = args[0];
            if (!configs.TryGetValue(name, out var runDefinition))
            {
                Logger.LogError("Unknown command: {0}", name);
                Logger.LogError("Supported commands:");
                foreach (var commandConfig in configs)
                {
                    Logger.LogError(commandConfig.Key);
                }

                throw new Exception($"Unknown command: {name}");
            }

            runDefinition.Config.ParseArguments(args.Skip(1));
            var builder = Host.CreateDefaultBuilder();

            return builder
               .ConfigureAppConfiguration(
                    bdx =>
                    {
                        if (runDefinition.Config is ICommandConfixExtented extended)
                        {
                            extended.Configure(bdx);
                        }
                    })
                .ConfigureServices((context, collection) =>
                {
                    collection.AddSingleton(new ExecutionContext(name, Config));
                    collection.Add(new ServiceDescriptor(runDefinition.Config.GetType(), ctx => runDefinition.Config, ServiceLifetime.Singleton));
                    collection.Add(runDefinition.Service);
                    runDefinition.Config.Build(collection, context.Configuration);
                    collection.AddHostedService<ExecutionHost>();
                });
        }
    }
}
