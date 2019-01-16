using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class SampleCommandTwo : Command
    {
        public SampleCommandTwo(ILogger<SampleCommandTwo> logger, ConfigTwo config)
            : base(logger)
        {
            Config = config;
        }

        public ConfigTwo Config { get; }

        protected override Task Execute(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
