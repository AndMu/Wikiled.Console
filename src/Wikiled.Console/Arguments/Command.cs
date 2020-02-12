using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wikiled.Console.Arguments
{
    /// <summary>
    /// Provides a base class for the functionality that all commands must implement.
    /// </summary>
    public abstract class Command
    {
        private readonly CancellationTokenSource executionToken = new CancellationTokenSource();

        private Task executionTask;

        private readonly Subject<bool> status = new Subject<bool>();

        protected Command(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IObservable<bool> Status => status;

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

        public virtual Task StartExecution(CancellationToken token)
        {
            executionTask = Task.Run(() => MainExecution(), executionToken.Token);
            return Task.CompletedTask;
        }

        public virtual async Task StopExecution(CancellationToken token)
        {
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
                await Execute(executionToken.Token).ConfigureAwait(false);
                status.OnNext(true);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed");
                status.OnNext(false);
            }
        }
    }
}