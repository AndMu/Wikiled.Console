using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter : IHostedService, IDisposable
    {
        string Name { get; }

        Command Command { get; }

        IObservable<bool> Status { get; }

        ServiceCollection Service { get; }

        IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new();
    }
}