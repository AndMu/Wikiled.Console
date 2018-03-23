using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Wikiled.Common.Arguments;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Command> commands;

        public AutoStarter(string name)
        {
            Guard.NotNullOrEmpty(() => name, name);
            Name = name;
            List<Command> commandsList = new List<Command>();
            foreach (var instance in GetInstances<Command>())
            {
                commandsList.Add(instance);
            }

            commands = commandsList.ToDictionary(item => item.Name, item => item, StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }

        public void Start(string[] args)
        {
            log.Info("Starting {0} version {1}...", Assembly.GetExecutingAssembly().GetName().Version, Name);
            if (args.Length == 0)
            {
                log.Warn("Please specify arguments");
                return;
            }

            if (args.Length == 0 ||
                !commands.TryGetValue(args[0], out var command))
            {
                log.Error("Please specify command");
                return;
            }

            try
            {
                command.ParseArguments(args.Skip(1));
                command.Execute();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                System.Console.ReadLine();
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
