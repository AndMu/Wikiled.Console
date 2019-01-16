using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class SampleCommandCompleted : Command
    {
        private ILogger<SampleCommand> logger;

        public SampleCommandCompleted(ILogger<SampleCommand> logger, ConfigOne config)
        {
            this.logger = logger;
            Config = config;
        }

        public ConfigOne Config { get; }

        protected override Task Execute(CancellationToken token)
        {
            OnCompleted(true);
            return Task.CompletedTask;
        }
    }
}
