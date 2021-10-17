using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly Dictionary<string, (ICommandConfig Config, ServiceDescriptor Service)> configs = new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<AutoStarter> log;

        private readonly Action<ILoggingBuilder> loggingBuilder;

        public AutoStarter(string name, Action<ILoggingBuilder> loggingBuilder)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Name = name;
            this.loggingBuilder = loggingBuilder ?? throw new ArgumentNullException(nameof(loggingBuilder));
            log  = LoggerFactory.Create(loggingBuilder).CreateLogger<AutoStarter>();
        }

        public string Name { get; }

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new()
        {
            var descriptor = new ServiceDescriptor(typeof(IHostedService), typeof(T), ServiceLifetime.Singleton);
            configs[name] = (new TConfig(), descriptor);
            return this;
        }

        public IHostBuilder Build(string[] args)
        {
            log.LogInformation("Starting {0} version {1}...", Assembly.GetEntryAssembly()?.GetName().Version, Name);
            if (args.Length == 0)
            {
                log.LogError("Please specify arguments");
                throw new Exception("Please specify arguments");
            }

            if (args.Length == 0)
            {
                log.LogError("Please specify command");
                throw new Exception("Please specify command");
            }

            if (!configs.TryGetValue(args[0], out var runDefinition))
            {
                log.LogError("Unknown command: {0}", args[0]);
                log.LogError("Supported:");
                foreach (var commandConfig in configs)
                {
                    log.LogError(commandConfig.Key);
                }

                throw new Exception($"Unknown command: {args[0]}");
            }

            runDefinition.Config.ParseArguments(args.Skip(1));
            var builder = Host.CreateDefaultBuilder();
            if (!string.IsNullOrEmpty(runDefinition.Config.Environment))
            {
                builder.UseEnvironment(runDefinition.Config.Environment);
            }

            return builder
                .ConfigureServices(collection =>
                {
                    collection.Add(new ServiceDescriptor(runDefinition.Config.GetType(), ctx => runDefinition.Config, ServiceLifetime.Singleton));
                    collection.Add(runDefinition.Service);
                }).ConfigureLogging(loggingBuilder);
        }
    }
}
