using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class SampleCommandTwo : Command
    {
        private ILogger<SampleCommandTwo> logger;

        public SampleCommandTwo(ILogger<SampleCommandTwo> logger, ConfigTwo config)
        {
            this.logger = logger;
            Config = config;
        }

        public ConfigTwo Config { get; }

        protected override Task Execute(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
