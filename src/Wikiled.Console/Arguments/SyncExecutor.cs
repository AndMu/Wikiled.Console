using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    public class SyncExecutor
    {
        private Task task;

        private CancellationTokenSource source;

        private readonly AutoStarter starter;

        public SyncExecutor(AutoStarter starter)
        {
            this.starter = starter ?? throw new ArgumentNullException(nameof(starter));
        }

        public async Task Execute()
        {
            source = new CancellationTokenSource();
            task = starter.StartAsync(source.Token);
            System.Console.WriteLine("Please press CTRL+C to break...");
            System.Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            await starter.Status.LastOrDefaultAsync();
            System.Console.WriteLine("Exiting...");
        }

        private async void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!task.IsCompleted)
            {
                source.Cancel();
            }

            source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await starter.StopAsync(source.Token).ConfigureAwait(false);
        }
    }
}
