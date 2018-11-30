using Microsoft.Extensions.Hosting;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter : IHostedService
    {
        string Name { get; }

        Command Command { get; }

        IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new();
    }
}