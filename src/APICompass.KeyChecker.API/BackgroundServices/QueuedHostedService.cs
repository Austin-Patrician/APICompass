using APICompass.KeyChecker.Infrastructure.Queue;
using APICompass.KeyChecker.Infrastructure.Metrics;

namespace APICompass.KeyChecker.API.BackgroundServices;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly int _workerCount;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger,
        IConfiguration configuration)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _workerCount = configuration.GetValue<int>("BackgroundWorkers:WorkerCount", Environment.ProcessorCount * 2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting with {WorkerCount} workers", _workerCount);

        var workers = new List<Task>();
        for (int i = 0; i < _workerCount; i++)
        {
            var workerId = i;
            workers.Add(Task.Run(async () => await ProcessQueueAsync(workerId, stoppingToken), stoppingToken));
        }

        await Task.WhenAll(workers);
    }

    private async Task ProcessQueueAsync(int workerId, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker {WorkerId} is starting", workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                if (workItem != null)
                {
                    try
                    {
                        ValidationMetrics.UpdateQueueDepth(_taskQueue.GetQueueDepth());
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing background work item");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in worker {WorkerId}", workerId);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Worker {WorkerId} is stopping", workerId);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
