using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    /// <summary>
    /// Provides a base class for the functionality that all commands must implement.
    /// </summary>
    public abstract class Command
    {
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
            return Execute(token);
        }

        public virtual Task StopExecution(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected abstract Task Execute(CancellationToken token);
    }
}