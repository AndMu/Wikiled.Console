using System;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Console.Arguments;

namespace Wikiled.Console.Tests.Data
{
    public class BlockingCommand : ICommand
    {
        public BlockingCommand(ConfigOne config)
        {
            Config = config;
        }

        public ConfigOne Config { get; }

        public int Stage { get; set; }

        public async Task Execute(CancellationToken token)
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
