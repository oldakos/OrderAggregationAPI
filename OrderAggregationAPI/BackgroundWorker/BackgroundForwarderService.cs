namespace OrderAggregationAPI.BackgroundWorker
{
    //see https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=visual-studio

    /// <summary>
    /// On a timer, sends Orders from a persistence provider using a sender.
    /// </summary>
    public class BackgroundForwarderService : IHostedService, IDisposable
    {
        private int _period_seconds;

        private readonly ILogger<BackgroundForwarderService> _logger;
        private Timer? _timer = null;
        private IOrderPersistenceProvider _orderPersistenceProvider;
        private IOrderSender _orderSender;

        public BackgroundForwarderService(BackgroundForwarderServiceConfig config, IOrderPersistenceProvider persistenceProvider, IOrderSender sender, ILogger<BackgroundForwarderService> logger)
        {
            _logger = logger;
            _orderPersistenceProvider = persistenceProvider;
            _period_seconds = config.PeriodSeconds;
            _orderSender = sender;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(_period_seconds), Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var order = _orderPersistenceProvider.Read();

            if (_orderSender.TrySend(order))
            {
                _orderPersistenceProvider.SubtractRange(order);
                //only delete persisted orders after core system confirms their reception
            }

            _timer?.Change(TimeSpan.FromSeconds(_period_seconds), Timeout.InfiniteTimeSpan);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service stopping.");

            _timer?.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
