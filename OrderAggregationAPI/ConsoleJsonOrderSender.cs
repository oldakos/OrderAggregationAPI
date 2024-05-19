
using System.Text.Json;
using OrderAggregationAPI.Dataclasses;

namespace OrderAggregationAPI
{
    public class ConsoleJsonOrderSender : IOrderSender
    {
        private readonly ILogger<ConsoleJsonOrderSender> _logger;

        public ConsoleJsonOrderSender(ILogger<ConsoleJsonOrderSender> logger)
        {
            _logger = logger;
        }

        public bool TrySend(IEnumerable<ProductQuantity> order)
        {
            //we consider empty orders unwanted by recipient
            //but allowed for any code that uses this sender class
            //therefore, don't actually send but return `true`
            if(order == null || order.Count() < 1)
            {
                return true;
            }

            try
            {
                var json = JsonSerializer.Serialize(order);

                _logger.LogInformation("Sending an order...");

                Console.WriteLine(json);

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
