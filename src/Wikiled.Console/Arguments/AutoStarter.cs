using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wikiled.Common.Arguments;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Command> commands;

        private readonly string[] args;

        private Command command = default;

        public AutoStarter(string name, string[] args)
        {
            Guard.NotNullOrEmpty(() => name, name);
            Guard.NotNull(() => args, args);
            Name = name;
            this.args = args;
            List<Command> commandsList = new List<Command>();
            foreach (var instance in GetInstances<Command>())
            {
                commandsList.Add(instance);
            }

            commands = commandsList.ToDictionary(item => item.Name, item => item, StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }

        public async Task Start(CancellationToken token)
        {
            log.Info("Starting {0} version {1}...", Assembly.GetEntryAssembly().GetName().Version, Name);
            if (args.Length == 0)
            {
                log.Warn("Please specify arguments");
                return;
            }

            if (args.Length == 0 ||
                !commands.TryGetValue(args[0], out command))
            {
                log.Error("Please specify command");
                return;
            }

            try
            {
                command.ParseArguments(args.Skip(1));
                await command.StartExecution(token);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async Task Stop(CancellationToken token)
        {
            log.Info("Request stopping");
            if (command != null)
            {
                await command.StopExecution(token);
            }
        }

        private static IEnumerable<T> GetInstances<T>()
        {
            return (from t in Assembly.GetEntryAssembly().GetTypes()
                    where t.IsSubclassOf(typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t));
        }
    }
}
