using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.Common.Utilities.Modules;

namespace Wikiled.Console.Arguments
{
    public class AutoStarter : IAutoStarter
    {
        private readonly Dictionary<string, ICommandConfig> configs = new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger<AutoStarter> log;

        private readonly string[] args;

        private readonly Subject<bool> status = new();

        private ServiceProvider container;

        private IDisposable commandStatus;

        private bool isDisposed;

        private bool isCompleted;

        private IConfiguration configuration;

        public AutoStarter(string name, string[] args, Action<ILoggingBuilder> logging)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Name = name;
            this.args = args ?? throw new ArgumentNullException(nameof(args));
            Service.AddLogging(logging);
            log = Service.BuildServiceProvider().GetRequiredService<ILogger<AutoStarter>>();
        }

        public ServiceCollection Service { get; } = new();

        public Command Command { get; private set; }

        public IObservable<bool> Status => status.Distinct();

        public string Name { get; }

        public Func<IServiceProvider, Task> Init { get; set; }

        public void SetupConfiguration(Action<IConfigurationBuilder> config)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            config?.Invoke(builder);
            configuration = builder.Build();
            Service.AddSingleton(configuration);
        }

        public IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new()
        {
            Service.AddScoped<Command, T>(name.ToLower());
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

            if (!configs.TryGetValue(args[0], out ICommandConfig config))
            {
                log.LogError("Unknown command: {0}", args[0]);
                log.LogError("Supported:");
                foreach (var commandConfig in configs)
                {
                    log.LogError(commandConfig.Key);
                }

                return;
            }

            try
            {
                OnStatus(true);
                config.ParseArguments(args.Skip(1));

                if (configuration == null)
                {
                    SetupConfiguration(null);
                }

                config.Build(Service, configuration);
                Service.AddSingleton(config.GetType(), ctx => config);
                container = Service.BuildServiceProvider();
                log.LogDebug("Resolving service");
                using var scope = container.CreateScope();
                if (Init != null)
                {
                    log.LogDebug("Initialisation routine");
                    await Init(scope.ServiceProvider).ConfigureAwait(false);
                }

                Command = scope.ServiceProvider.GetService<Command>(args[0].ToLower());
                commandStatus = Command.Status.Subscribe(item =>
                {
                    log.LogInformation(
                        "Command completed: {0}",
                        item ? "Successfully" : "Failed");
                    OnStatus(false);
                    Completed();
                });

                log.LogDebug("Starting execution");
                await Command.StartExecution(token).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Failed");
                OnStatus(false);
                Completed();
                throw;
            }
        }

        public async Task StopAsync(CancellationToken token)
        {
            log?.LogInformation("Request stopping");
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
