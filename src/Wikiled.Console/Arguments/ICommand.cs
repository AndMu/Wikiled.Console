using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments;

public interface ICommand
{
    Task Execute(CancellationToken token);
}