using Autofac;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class ConfigTwo : ICommandConfig
    {
        public string Data { get; set; }

        public void Build(ContainerBuilder builder)
        {
        }
    }
}
