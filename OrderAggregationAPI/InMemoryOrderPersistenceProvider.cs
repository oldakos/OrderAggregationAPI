﻿
using Microsoft.AspNetCore.Components.Forms.Mapping;
using OrderAggregationAPI.Dataclasses;
using System.Collections.Concurrent;

namespace OrderAggregationAPI
{
    /// <summary>
    /// An in-memory container that "adds up" ProductQuantity objects with thread-safe access.
    /// </summary>
    public class InMemoryOrderPersistenceProvider : IOrderPersistenceProvider
    {
        private ConcurrentDictionary<string, int> _products = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Apply the given Func to the amount of the given product.
        /// </summary>
        /// <param name="key">The productId</param>
        /// <param name="updateFunc">The Func to be applied to the amount of the given productId</param>
        /// <returns>`true` if the value was updated. `false` if the ID doesn't exist.</returns>
        private bool TryUpdate(string key, Func<int, int> updateFunc)
        {
            int currentValue;
            while (_products.TryGetValue(key, out currentValue))
            {
                if (_products.TryUpdate(key, updateFunc(currentValue), currentValue))
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(ProductQuantity productQuantity)
        {
            var key = productQuantity.productId;
            var value = productQuantity.quantity;

            if (_products.TryAdd(key, value))
            {
                return;
            }

            var updateFunc = (int q) => q + value;
            TryUpdate(key, updateFunc); //this line may do nothing and return false if the key doesn't exist. but we checked the key's existence in this method and we generally do not remove keys in this class.
        }

        public void AddRange(IEnumerable<ProductQuantity> productQuantities)
        {
            foreach (var productQuantity in productQuantities)
            {
                Add(productQuantity);
            }
        }

        public IEnumerable<ProductQuantity> Read()
        {
            var keys = _products.Keys;
            var result = new List<ProductQuantity>();
            foreach (var key in keys)
            {
                int value;
                if (_products.TryGetValue(key, out value) && value > 0)
                {
                    result.Add(new ProductQuantity(key, value));
                }
            }
            return result;
        }

        public void Subtract(ProductQuantity productQuantity)
        {
            var key = productQuantity.productId;
            var value = productQuantity.quantity;

            if (_products.TryAdd(key, value))
            {
                return;
            }

            var updateFunc = (int q) => q - value;
            TryUpdate(key, updateFunc); //this line may do nothing and return false if the key doesn't exist. but we checked the key's existence in this method and we generally do not remove keys in this class.
        }

        public void SubtractRange(IEnumerable<ProductQuantity> productQuantities)
        {
            foreach (var productQuantity in productQuantities)
            {
                Subtract(productQuantity);
            }
        }
    }
}
