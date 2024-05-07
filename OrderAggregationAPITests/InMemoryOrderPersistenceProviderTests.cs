using OrderAggregationAPI;
using OrderAggregationAPI.Dataclasses;
using System.Collections;

namespace OrderAggregationAPITests
{
    public class InMemoryOrderPersistenceProviderTests
    {
        InMemoryOrderPersistenceProvider _provider;

        public InMemoryOrderPersistenceProviderTests()
        {
            _provider = new InMemoryOrderPersistenceProvider();
        }

        [Fact]
        public void WithNothingAdded_ReadShouldReturnEmpty()
        {
            var readOrder = _provider.Read();

            Assert.Empty(readOrder);
        }

        [Theory]
        [ClassData(typeof(RandomOrderData))]
        public void WithSingleOrderAdded_ReadShouldReturnTheSame(IEnumerable<ProductQuantity> order)
        {
            _provider.AddRange(order);
            var readOrder = _provider.Read();

            //the order in InMemoryOrderPersistenceProvider isn't really defined, so let's reorder the collections before comparing
            Assert.True(order.OrderBy(p => p.productId).SequenceEqual(readOrder.OrderBy(p => p.productId)));
        }

        [Theory]
        [ClassData(typeof(RandomOrderData))]
        public void WithSameOrderAddedAndSubtracted_ReadShouldReturnEmpty(IEnumerable<ProductQuantity> order)
        {
            _provider.AddRange(order);
            _provider.SubtractRange(order);
            var readOrder = _provider.Read();

            Assert.Empty(readOrder);
        }

        [Theory]
        [InlineData(["key", new int[] { 1, 2, 3, 4 }])]
        [InlineData(["456", new int[] { 45, 55 }])]
        [InlineData(["Mahogany Table", new int[] { 1_000_000, 1_000, 1 }])]
        public void WithSameProductAddedOverSeveralOrders_ReadReturnsCorrectSum(string productName, int[] quantities)
        {
            int sum = 0;
            foreach (var quantity in quantities)
            {
                sum += quantity;
                _provider.Add(new ProductQuantity(productName, quantity));
            }
            var readOrder = _provider.Read();

            Assert.Equal(sum, readOrder.First().quantity);
        }

        [Theory]
        [ClassData(typeof(RandomOrderData))]
        public void WithAddRange_ShouldReadTheSameAsWithEquivalentIndividualAdds(IEnumerable<ProductQuantity> order)
        {
            //just get local providers for clarity, the test class only provides one
            var provider1 = new InMemoryOrderPersistenceProvider();
            var provider2 = new InMemoryOrderPersistenceProvider();

            provider1.AddRange(order);
            foreach (var product in order)
            {
                provider2.Add(product);
            }
            var readOrder1 = provider1.Read();
            var readOrder2 = provider2.Read();

            //the order in InMemoryOrderPersistenceProvider isn't really defined, so let's reorder the collections before comparing
            Assert.True(readOrder1.OrderBy(p => p.productId).SequenceEqual(readOrder2.OrderBy(p => p.productId)));
        }
    }

    public class RandomOrderData : IEnumerable<object[]>
    {
        public RandomOrderData() { }
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [new ProductQuantity[] { new("a", 1), new("b", 2), new("c", 3) }];
            yield return [new ProductQuantity[] { new("Pink basketball", 12), new("Trash bag 20l", 6) }];
            yield return [new ProductQuantity[] { new("456", 5), new("789", 42) }];
            yield return [new ProductQuantity[] { new("f", 1) }];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}