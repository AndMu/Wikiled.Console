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

        private readonly ExecutionContext executionContext;

        public ExecutionHost(ILogger<ExecutionHost> logger, IHostApplicationLifetime appLifetime, ICommand command, ExecutionContext executionContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            this.command = command ?? throw new ArgumentNullException(nameof(command));
            this.executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        }

        public Task StartAsync(CancellationToken token)
        {
            if (executionContext.Config.ValidateBeforeExecution)
            {
                logger.LogInformation("{0}: Do you want to continue? (y/n)", executionContext.CommandName);
                var response = System.Console.ReadLine();
                if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Aborting...");
                    appLifetime.StopApplication();
                    return Task.CompletedTask;
                }
            }

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
                await command.Execute(executionToken.Token);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed");
            }

            appLifetime.StopApplication();
        }
    }
}