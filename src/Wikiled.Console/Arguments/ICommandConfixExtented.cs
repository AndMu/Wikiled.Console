using Microsoft.Extensions.Configuration;

namespace Wikiled.Console.Arguments
{
    public interface ICommandConfixExtented : ICommandConfig
    {
        void Configure(IConfigurationBuilder builder);
    }
}
