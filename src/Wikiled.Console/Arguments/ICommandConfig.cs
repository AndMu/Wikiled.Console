using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wikiled.Console.Arguments
{
    public interface ICommandConfig
    {
        string Environment { get; set; }

        void Build(IServiceCollection services, IConfiguration config);
    }
}
