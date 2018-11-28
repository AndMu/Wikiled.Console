using Autofac;

namespace Wikiled.Console.Arguments
{
    public interface ICommandConfig
    {
        void Build(ContainerBuilder builder);
    }
}
