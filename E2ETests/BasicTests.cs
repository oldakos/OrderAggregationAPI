using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderAggregationAPI;
using OrderAggregationAPI.BackgroundWorker;
using OrderAggregationAPI.Dataclasses;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.Http.Json;
using System.Text.Json;

// see https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0

namespace E2ETests
{
    public class BasicTests : IClassFixture<OutputReadingWebApplicationFactory<OrderAggregationAPI.Program>>
    {
        private readonly OutputReadingWebApplicationFactory<Program> _factory;

        public BasicTests(OutputReadingWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task BasePath_Returns200AndSomeText()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/");

            response.EnsureSuccessStatusCode();
            Assert.Contains("text", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task OneOrder_GetsSentUnchanged()
        {
            var client = _factory.CreateClient();
            var order = new ProductQuantity[] { new ProductQuantity("myId", 42) };

            await client.PostAsync("/order", JsonContent.Create(order));
            _factory.SleepUntilOutput();

            var outputOrder = _factory.DequeueOutput();

            Assert.Equal(order, outputOrder);
        }

        [Theory]
        [InlineData("myId",1,2)]
        public async Task TwoOrders_GetAddedTogether(string id, int quant1, int quant2)
        {
            var client = _factory.CreateClient();
            var order1 = new ProductQuantity[] { new ProductQuantity(id, quant1) };
            var order2 = new ProductQuantity[] { new ProductQuantity(id, quant2) };

            await client.PostAsync("/order", JsonContent.Create(order1));
            await client.PostAsync("/order", JsonContent.Create(order2));

            _factory.SleepUntilOutput();
            var outputOrder = _factory.DequeueOutput();

            Assert.Equal(quant1 + quant2, outputOrder.First().quantity);
        }
    }

    /// <summary>
    /// A custom WebApplicationFactory that configures an IOrderSender whose output can be observed by test classes.
    /// </summary>
    /// <typeparam name="TProgram">The entry point class of the system under test.</typeparam>
    public class OutputReadingWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private const int PERIOD_SECONDS = 1;

        private ConcurrentQueue<IEnumerable<ProductQuantity>> _queue;

        public OutputReadingWebApplicationFactory()
        {
            _queue = new ConcurrentQueue<IEnumerable<ProductQuantity>>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var orderSenderDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOrderSender));
                services.Remove(orderSenderDescriptor);
                services.AddSingleton<IOrderSender>(container =>
                {
                    var logger = container.GetRequiredService<ILogger<InMemoryQueueJsonOrderSender>>();
                    var sender = new InMemoryQueueJsonOrderSender(logger, _queue);
                    return sender;
                });
                var backgroundConfigDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(BackgroundForwarderServiceConfig));
                services.Remove(orderSenderDescriptor);
                services.AddSingleton(container =>
                {
                    var config = new BackgroundForwarderServiceConfig();
                    config.PeriodSeconds = PERIOD_SECONDS;
                    return config;
                });
            });
        }

        public IEnumerable<ProductQuantity> DequeueOutput()
        {
            IEnumerable<ProductQuantity> output;
            if (_queue.TryDequeue(out output))
            {
                return output;
            }
            else
            {
                return Array.Empty<ProductQuantity>();
            }
        }

        /// <summary>
        /// Pass enough time to ensure that an output from the aggregator would have been produced.
        /// </summary>
        public void SleepUntilOutput()
        {
            Thread.Sleep(PERIOD_SECONDS * 1000 + 100);
        }
    }




}