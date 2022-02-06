using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class ConfigTwo : ICommandConfig
    {
        public string Data { get; set; }

        public void Build(IServiceCollection services, IConfiguration configuration)
        {
        }
    }
}
