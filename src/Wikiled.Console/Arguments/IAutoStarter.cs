using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter
    {
        string Name { get; }

        Task Start(CancellationToken token);

        Task Stop(CancellationToken token);
    }
}