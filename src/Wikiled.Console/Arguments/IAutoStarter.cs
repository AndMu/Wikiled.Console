using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    public interface IAutoStarter
    {
        string Name { get; }

        Task Start();
    }
}