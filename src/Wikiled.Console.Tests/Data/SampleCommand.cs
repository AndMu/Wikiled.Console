using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data;

public class SampleCommand : ICommand
{
    public SampleCommand(ConfigOne config)
    {
        Config = config;
    }

    public ConfigOne Config { get; }

    public Task Execute(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}