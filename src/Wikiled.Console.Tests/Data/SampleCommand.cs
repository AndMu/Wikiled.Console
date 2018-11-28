using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class SampleCommand : Command
    {
        private ILogger<SampleCommand> logger;

        public SampleCommand(ILogger<SampleCommand> logger, ConfigOne config)
        {
            this.logger = logger;
            Config = config;
        }

        public ConfigOne Config { get; }

        protected override Task Execute(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
