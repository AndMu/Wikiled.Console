using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data;

public class SampleCommandTwo : ICommand
{
    public SampleCommandTwo(ConfigTwo config)
    {
        Config = config;
    }

    public ConfigTwo Config { get; }

    public Task Execute(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}