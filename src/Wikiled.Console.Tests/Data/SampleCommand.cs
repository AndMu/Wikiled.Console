using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class SampleCommand : Command
    {
        public SampleCommand(ILogger<SampleCommand> logger, ConfigOne config)
            : base(logger)
        {
            Config = config;
        }

        public ConfigOne Config { get; }

        protected override Task Execute(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
