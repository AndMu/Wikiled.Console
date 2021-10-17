using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    /// <summary>
    /// Provides a base class for the functionality that all commands must implement.
    /// </summary>
    public abstract class Command : IHostedService
    {
        private readonly CancellationTokenSource executionToken = new();

        private Task executionTask;

        protected Command(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ILogger Logger { get; }

        /// <summary>
        /// The name of the command. The base implementation is to strip off the last
        /// instance of "Command" from the end of the type name. So "DiscoverCommand"
        /// would become "Discover". If the type name does not have the string "Command" in it, 
        /// then the name of the command is the same as the type name. This behavior can be 
        /// overridden, but most derived classes are going to be of the form [Command Name] + Command.
        /// </summary>
        public virtual string Name
        {
            get
            {
                string typeName = GetType().Name;
                if (typeName.Contains("Command"))
                {
                    return typeName.Remove(typeName.LastIndexOf("Command", StringComparison.Ordinal));
                }

                return typeName;
            }
        }

        public virtual Task StartAsync(CancellationToken token)
        {
            Logger.LogDebug("StartExecution");
            executionTask = Task.Run(MainExecution, executionToken.Token);
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken token)
        {
            Logger.LogDebug("StopExecution");
            executionToken.Cancel();
            try
            {
                if (executionTask != null)
                {
                    await executionTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected abstract Task Execute(CancellationToken token);

        private async Task MainExecution()
        {
            try
            {
                Logger.LogDebug("MainExecution");
                await Execute(executionToken.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed");
            }
        }
    }
}