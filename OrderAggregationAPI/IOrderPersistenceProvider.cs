using OrderAggregationAPI.Dataclasses;

namespace OrderAggregationAPI
{
    /// <summary>
    /// An object that can aggregate orders. Implementors must be thread-safe!
    /// </summary>
    public interface IOrderPersistenceProvider
    {
        /// <summary>
        /// Add a quantity of a single product to the aggregated order.
        /// </summary>
        /// <param name="productQuantity">The `ProductQuantity` to add.</param>
        public void Add(ProductQuantity productQuantity);
        /// <summary>
        /// Add quantities of several products to the aggregated order.
        /// </summary>
        /// <param name="productQuantities">The `ProductQuantity` objects to add.</param>
        public void AddRange(IEnumerable<ProductQuantity> productQuantities);
        /// <summary>
        /// Get the quantities of products currently in the aggregated order.
        /// </summary>
        /// <returns>A `ProductQuantity` for each product of a positive quantity.</returns>
        public IEnumerable<ProductQuantity> Read();
        /// <summary>
        /// Subtract a quantity of a single product from the aggregated order.
        /// </summary>
        /// <param name="productQuantity">The `ProductQuantity` to subtract.</param>
        public void Subtract(ProductQuantity productQuantity);
        /// <summary>
        /// Subtract quantities of several products from the aggregated order.
        /// </summary>
        /// <param name="productQuantities">The `ProductQuantity` objects to subtract.</param>
        public void SubtractRange(IEnumerable<ProductQuantity> productQuantities);
    }
}
