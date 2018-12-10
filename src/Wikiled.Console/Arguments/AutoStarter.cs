using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public AutoStarter(string name, string[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Name = name;
            this.args = args ?? throw new ArgumentNullException(nameof(args));
            log = ApplicationLogging.LoggerFactory.CreateLogger<AutoStarter>();
        }

        public Command Command { get; private set; }

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

            if (args.Length == 0 || !configs.TryGetValue(args[0].ToLower(), out ICommandConfig config))
            {
                log.LogError("Please specify command");
                return Task.CompletedTask;
            }

            config.ParseArguments(args.Skip(1));
            builder.RegisterModule(new LoggingModule(ApplicationLogging.LoggerFactory));
            config.Build(builder);
            builder.RegisterInstance(config).As(config.GetType());
            container = builder.Build();
            Command = container.ResolveNamed<Command>(args[0].ToLower());
            return Task.Run(() => Command.StartExecution(token), token);
        }

        public async Task StopAsync(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            if (Command != null)
            {
                await Command.StopExecution(token);
            }

            container?.Dispose();
        }
    }
}
