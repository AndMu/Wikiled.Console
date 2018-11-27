using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter
    {
        string Name { get; }

        IAutoStarter Register<T>(string name)
            where T : Command;

        IAutoStarter RegisterModule(Module module);

        Task Start(string[] args, CancellationToken token);

        Task Stop(CancellationToken token);
    }
}