using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Module = Autofac.Module;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly ILogger<AutoStarter> log;

        private Command command;

        private readonly ContainerBuilder builder = new ContainerBuilder();

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
            Name = name;
            log = factory.CreateLogger<AutoStarter>();
        }

        public string Name { get; }

        public IAutoStarter Register<T>(string name)
            where T : Command
        {
            builder.RegisterType<T>().Named<Command>(name);
            return this;
        }

        public IAutoStarter RegisterModule(Module module)
        {
            builder.RegisterModule(module);
            return this;
        }

        public async Task Start(string[] args, CancellationToken token)
        {
            var container = builder.Build();
            log.LogInformation("Starting {0} version {1}...", Assembly.GetEntryAssembly().GetName().Version, Name);
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

            try
            {
                command = container.ResolveNamed<Command>(args[0]);
                command.ParseArguments(args.Skip(1));
                await command.StartExecution(token);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
            }
        }

        public async Task Stop(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            if (command != null)
            {
                await command.StopExecution(token);
            }
        }
    }
}
