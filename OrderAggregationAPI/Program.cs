using OrderAggregationAPI.BackgroundWorker;
using OrderAggregationAPI.Dataclasses;
using System.IO;

namespace OrderAggregationAPI
{
    public partial class Program
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

        static void BasicInformation(HttpContext context)
        {
            context.Response.ContentType = "text/plain; charset=UTF-8";
            context.Response.WriteAsync("Use the `POST /order` endpoint to submit your order.");
        }

        static IResult CreateOrder(List<ProductQuantity> items, IOrderPersistenceProvider persistenceProvider)
        {
            persistenceProvider.AddRange(items);
            return TypedResults.Ok();
        }
    }
}
