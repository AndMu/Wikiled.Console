using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Logging;
using Wikiled.Common.Utilities.Modules;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly ILogger<AutoStarter> log;

        private readonly ContainerBuilder builder = new ContainerBuilder();

        private readonly Dictionary<string, ICommandConfig> configs = new Dictionary<string, ICommandConfig>(StringComparer.OrdinalIgnoreCase);

        private readonly string[] args;

        private IContainer container;

        private readonly Subject<bool> status = new Subject<bool>();

        public AutoStarter(ILoggerFactory factory, string name, string[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Name = name;
            this.args = args ?? throw new ArgumentNullException(nameof(args));
            LoggerFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            log = ApplicationLogging.LoggerFactory.CreateLogger<AutoStarter>();
        }

        public ILoggerFactory LoggerFactory { get; }

        public Command Command { get; private set; }

        public IObservable<bool> Status => status;

        public string Name { get; }

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new()
        {
            builder.RegisterType<T>().Named<Command>(name.ToLower());
            configs[name] = new TConfig();
            return this;
        }

        public Task StartAsync(CancellationToken token)
        {
            log.LogInformation("Starting {0} version {1}...", Assembly.GetEntryAssembly()?.GetName().Version, Name);
            if (args.Length == 0)
            {
                log.LogWarning("Please specify arguments");
                return Task.CompletedTask;
            }

            if (args.Length == 0)
            {
                log.LogError("Please specify command");
                return Task.CompletedTask;
            }

            if (!configs.TryGetValue(args[0].ToLower(), out ICommandConfig config))
            {
                log.LogError("Unknown command: {0}", args[0]);
                log.LogError("Supported:");
                foreach (var commandConfig in configs)
                {
                    log.LogError(commandConfig.Key);
                }

                return Task.CompletedTask;
            }

            status.OnNext(true);
            config.ParseArguments(args.Skip(1));
            builder.RegisterModule(new LoggingModule(LoggerFactory));
            config.Build(builder);
            builder.RegisterInstance(config).As(config.GetType());
            container = builder.Build();
            Command = container.ResolveNamed<Command>(args[0].ToLower());
            return Command.StartExecution(token);
        }

        public async Task StopAsync(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            status.OnNext(false);
            if (Command != null)
            {
                await Command.StopExecution(token);
            }

            status.OnCompleted();
            container?.Dispose();
        }
    }
}
