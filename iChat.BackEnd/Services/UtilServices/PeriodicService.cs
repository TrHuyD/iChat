using Microsoft.Extensions.Hosting;

namespace iChat.BackEnd.Services.UtilServices
{
    public abstract class PeriodicService : BackgroundService
    {
        private readonly TimeSpan _interval;

        protected PeriodicService(TimeSpan interval)
        {
            _interval = interval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (EnableRequirement())
                        await ExecuteTask();

                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        protected virtual void OnError(Exception ex)
        {
            Console.Error.WriteLine($"[PeriodicService] {ex}");
        }

        protected abstract bool EnableRequirement();
        protected abstract Task ExecuteTask();
    }
}
