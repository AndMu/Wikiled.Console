using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.Common.Logging;
using Wikiled.Common.Utilities.Modules;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly ILogger<AutoStarter> log;

        private readonly Dictionary<string, ICommandConfig> configs = new Dictionary<string, ICommandConfig>(StringComparer.OrdinalIgnoreCase);

        private readonly string[] args;

        private readonly Subject<bool> status = new Subject<bool>();

        private readonly ServiceCollection service = new ServiceCollection();

        private ServiceProvider container;

        private IDisposable commandStatus;

        private bool isDisposed;

        private bool isCompleted;

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

        public IObservable<bool> Status => status.Distinct();

        public string Name { get; }

        public Func<IServiceProvider, Task> Init { get; set; }

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new()
        {
            service.AddScoped<Command, T>(name.ToLower());
            configs[name] = new TConfig();
            return this;
        }

        public async Task StartAsync(CancellationToken token)
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

            if (!configs.TryGetValue(args[0].ToLower(), out ICommandConfig config))
            {
                log.LogError("Unknown command: {0}", args[0]);
                log.LogError("Supported:");
                foreach (var commandConfig in configs)
                {
                    log.LogError(commandConfig.Key);
                }

                return;
            }

            OnStatus(true);
            config.ParseArguments(args.Skip(1));
            service.RegisterModule(new LoggingModule(LoggerFactory));
            config.Build(service);
            service.AddSingleton(config.GetType(), ctx => config);
            container = service.BuildServiceProvider();
            Command = container.GetService<Command>(args[0].ToLower());
            if (Init != null)
            {
                await Init(container).ConfigureAwait(false);
            }

            commandStatus = Command.Status.Subscribe(item =>
            {
                log.LogInformation(
                    "Command completed: {0}",
                    item ? "Successfully" : "Failed");
                OnStatus(false);
                Completed();
            });

            await Command.StartExecution(token).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken token)
        {
            log.LogInformation("Request stopping");
            OnStatus(false);
            if (Command != null)
            {
                await Command.StopExecution(token);
            }

            Completed();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            Completed();
            commandStatus?.Dispose();
            container?.Dispose();
            status?.Dispose();
        }

        private void OnStatus(bool value)
        {
            if (isCompleted)
            {
                return;
            }

            status.OnNext(value);
        }

        private void Completed()
        {
            if (isCompleted)
            {
                return;
            }

            isCompleted = true;
            status.OnCompleted();
        }
    }
}
