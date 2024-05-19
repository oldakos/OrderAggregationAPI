namespace OrderAggregationAPI.Dataclasses
{
    /// <summary>
    /// Represents a number of items of a product ID.
    /// </summary>
    /// <param name="productId">Any string.</param>
    /// <param name="quantity">The number of items.</param>
    public record struct ProductQuantity(string productId, int quantity);
    //note: IDE gives naming violation warning, but we're using lowercase to simplify JSON serialization code
}
