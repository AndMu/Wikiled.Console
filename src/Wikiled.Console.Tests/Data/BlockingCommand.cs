using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class BlockingCommand : Command
    {
        private ILogger<BlockingCommand> logger;

        public BlockingCommand(ILogger<BlockingCommand> logger, ConfigOne config)
        {
            this.logger = logger;
            Config = config;
        }

        public ConfigOne Config { get; }

        public int Stage { get; set; }

        protected override async Task Execute(CancellationToken token)
        {
            Stage = 1;
            try
            {
                await Task.Delay(10000, token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Stage = 2;
                throw;
            }

            Stage = 3;
        }
    }
}
