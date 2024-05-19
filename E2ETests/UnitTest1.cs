using Microsoft.AspNetCore.Mvc.Testing;
using OrderAggregationAPI;

// see https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0

namespace E2ETests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<OrderAggregationAPI.Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicTests(WebApplicationFactory<Program> factory)
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


    }
}