using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly ILogger<AutoStarter> log;

        private readonly ContainerBuilder builder = new ContainerBuilder();

        private readonly Dictionary<string, ICommandConfig> configs = new Dictionary<string, ICommandConfig>(StringComparer.OrdinalIgnoreCase);

        public AutoStarter(ILoggerFactory factory, string name)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            builder.RegisterInstance(factory);
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(); 
            builder.Populate(services); 

            Name = name;
            log = factory.CreateLogger<AutoStarter>();
        }

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

        public async Task Start(string[] args, CancellationToken token)
        {
            log.LogInformation("Starting {0} version {1}...", Assembly.GetEntryAssembly()?.GetName().Version, Name);
            if (args.Length == 0)
            {
                log.LogWarning("Please specify arguments");
                return;
            }

            if (args.Length == 0)
            {
                log.LogError("Please specify command");
                return;
            }

            if (args.Length == 0 ||
                !configs.TryGetValue(args[0], out var config))
            {
                log.LogError("Please specify command");
                return;
            }

            try
            {
                config.ParseArguments(args.Skip(1));
                config.Build(builder);
                builder.RegisterInstance(config).As(config.GetType());
                var container = builder.Build();
                Command = container.ResolveNamed<Command>(args[0]);
                await Command.StartExecution(token);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
            }
        }

        public async Task Stop(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            if (Command != null)
            {
                await Command.StopExecution(token);
            }
        }
    }
}
