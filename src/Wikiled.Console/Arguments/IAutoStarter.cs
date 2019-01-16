using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter : IHostedService, IDisposable
    {
        string Name { get; }

        Command Command { get; }

        ILoggerFactory LoggerFactory { get; }

        IObservable<bool> Status { get; }

        IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new();
    }
}