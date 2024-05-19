using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using OrderAggregationAPI.Dataclasses;

namespace OrderAggregationAPI
{
    /// <summary>
    /// Writes given orders to a provided ConcurrentQueue. Intended for testing.
    /// </summary>
    public class InMemoryQueueJsonOrderSender : IOrderSender
    {
        private readonly ILogger<InMemoryQueueJsonOrderSender> _logger;
        private readonly ConcurrentQueue<IEnumerable<ProductQuantity>> _queue;

        public InMemoryQueueJsonOrderSender(ILogger<InMemoryQueueJsonOrderSender> logger, ConcurrentQueue<IEnumerable<ProductQuantity>> queue)
        {
            _logger = logger;
            _queue = queue;
        }

        public bool TrySend(IEnumerable<ProductQuantity> order)
        {
            if (order == null || order.Count() < 1)
            {
                return true;
            }

            try
            {
                _logger.LogInformation("Sending an order...");

                _queue.Enqueue(order);

                _logger.LogInformation("Order sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Order could not be sent due to the following exception: {ex}");
                return false;
            }
            return true;
        }
    }
}
