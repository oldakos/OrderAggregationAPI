using OrderAggregationAPI.BackgroundWorker;
using OrderAggregationAPI.Dataclasses;

namespace OrderAggregationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddLogging(builder => builder.AddConsole());
            builder.Services.AddSingleton<IOrderPersistenceProvider, InMemoryOrderPersistenceProvider>();
            builder.Services.AddSingleton<IOrderSender, ConsoleJsonOrderSender>();
            builder.Services.AddSingleton(new BackgroundForwarderServiceConfig());
            builder.Services.AddHostedService<BackgroundForwarderService>();

            var app = builder.Build();

            app.MapGet("/", BasicInformation);
            app.MapPost("/order", CreateOrder);

            app.Run();
        }

        static IResult BasicInformation()
        {
            return TypedResults.Ok("Use the `POST /order` endpoint to submit your order.");
        }

        static IResult CreateOrder(List<ProductQuantity> items, IOrderPersistenceProvider persistenceProvider)
        {
            persistenceProvider.AddRange(items);
            return TypedResults.Ok();
        }
    }
}
