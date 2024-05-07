using OrderAggregationAPI.Dataclasses;

namespace OrderAggregationAPI
{
    /// <summary>
    /// An object able to communicate with our core system and deliver orders to it.
    /// </summary>
    public interface IOrderSender
    {
        /// <summary>
        /// Try to send the given order to the core system, which may or may not accept it.
        /// </summary>
        /// <param name="order">A collection of products and their quantities.</param>
        /// <returns>`true` if sending was successful, `false` otherwise.</returns>
        public bool TrySend(IEnumerable<ProductQuantity> order);
    }
}