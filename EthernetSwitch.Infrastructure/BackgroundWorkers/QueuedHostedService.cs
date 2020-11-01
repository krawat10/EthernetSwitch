using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EthernetSwitch.BackgroundWorkers
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger<QueuedHostedService> _logger;
        private CancellationToken cancellationToken;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            TaskQueue = taskQueue;
            _logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cancellationToken = stoppingToken;
            _logger.LogInformation(
                $"Queued Hosted Service is running");

            await BackgroundProcessing();
        }

        private async Task BackgroundProcessing()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem =
                    await TaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}