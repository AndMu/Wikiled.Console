using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wikiled.Console.Arguments
{
    public sealed class ExecutionHost : IHostedService
    {
        private readonly CancellationTokenSource executionToken = new();

        private readonly IHostApplicationLifetime appLifetime;

        private Task executionTask;

        private readonly ILogger<ExecutionHost> logger;

        private readonly ICommand command;

        public ExecutionHost(ILogger<ExecutionHost> logger, IHostApplicationLifetime appLifetime, ICommand command)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            this.command = command ?? throw new ArgumentNullException(nameof(command));
        }
        

        public Task StartAsync(CancellationToken token)
        {
            logger.LogDebug("StartExecution");
            executionTask = Task.Run(MainExecution, executionToken.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken token)
        {
            logger.LogDebug("StopExecution");
            if (executionToken.IsCancellationRequested)
            {
                return;
            }

            executionToken.Cancel();
            try
            {
                if (executionTask != null)
                {
                    await executionTask;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task MainExecution()
        {
            try
            {
                logger.LogDebug("MainExecution");
                await command.Execute(executionToken.Token);
                appLifetime.StopApplication();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed");
            }
        }
    }
}