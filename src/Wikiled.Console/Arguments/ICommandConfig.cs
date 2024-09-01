using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wikiled.Console.Arguments;

public interface ICommandConfig
{
    void Build(IServiceCollection services, IConfiguration config);
}