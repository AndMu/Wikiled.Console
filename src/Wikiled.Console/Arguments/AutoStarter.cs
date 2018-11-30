using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Utilities.Modules;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly ILogger<AutoStarter> log;

        private readonly ContainerBuilder builder = new ContainerBuilder();

        private readonly Dictionary<string, ICommandConfig> configs = new Dictionary<string, ICommandConfig>(StringComparer.OrdinalIgnoreCase);

        private string[] args;

        public AutoStarter(string name, string[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }
            
            Name = name;
            this.args = args ?? throw new ArgumentNullException(nameof(args));
            log = Factory.CreateLogger<AutoStarter>();
        }

        public ILoggerFactory Factory { get; } = new LoggerFactory();

        public Command Command { get; private set; }

        public string Name { get; }

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new()
        {
            builder.RegisterType<T>().Named<Command>(name);
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

            if (args.Length == 0 ||
                !configs.TryGetValue(args[0], out ICommandConfig config))
            {
                log.LogError("Please specify command");
                return Task.CompletedTask;
            }

            try
            {
                config.ParseArguments(args.Skip(1));
                builder.RegisterModule(new LoggingModule(Factory));
                config.Build(builder);
                builder.RegisterInstance(config).As(config.GetType());
                using (IContainer container = builder.Build())
                {
                    Command = container.ResolveNamed<Command>(args[0]);
                    return Task.Run(() => Command.StartExecution(token), token);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            if (Command != null)
            {
                await Command.StopExecution(token);
            }
        }
    }
}
