using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter
    {
        string Name { get; }
        Command Command { get; }

        IAutoStarter RegisterCommand<T, TConfig>(string name)
            where T : Command
            where TConfig : ICommandConfig, new();

        Task Start(string[] args, CancellationToken token);

        Task Stop(CancellationToken token);
    }
}