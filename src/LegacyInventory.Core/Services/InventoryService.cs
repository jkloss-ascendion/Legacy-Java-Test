using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using LegacyInventory.Core.Data;
using LegacyInventory.Core.Models;

namespace LegacyInventory.Core.Services
{
    public class InventoryService
    {
        private readonly ProductRepository _repository;
        private readonly PriceFeedClient _priceFeed;
        private readonly ProductCache _cache;

        public InventoryService()
        {
            _repository = new ProductRepository();
            _priceFeed = new PriceFeedClient();

            var minutes = int.Parse(ConfigurationManager.AppSettings["CacheMinutes"] ?? "10");
            var cacheFile = Path.Combine(Path.GetTempPath(), "legacy-inventory-products.bin");
            _cache = new ProductCache(cacheFile, TimeSpan.FromMinutes(minutes));
        }

        public IList<Product> GetProducts()
        {
            var cached = _cache.Load();
            if (cached != null)
            {
                return cached;
            }

            var products = _repository.GetAll();
            _cache.Save(products);
            return products;
        }

        public Product GetProduct(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
            {
                return null;
            }

            var latest = _priceFeed.GetLatestPrice(product.Sku);
            if (latest.HasValue)
            {
                product.UnitPrice = latest.Value;
            }

            return product;
        }

        public bool AdjustStock(int id, int delta)
        {
            var product = _repository.GetById(id);
            if (product == null || product.QuantityOnHand + delta < 0)
            {
                return false;
            }

            _repository.UpdateQuantity(id, product.QuantityOnHand + delta);
            AuditLogger.Record("AdjustStock", string.Format("id={0} delta={1}", id, delta));
            return true;
        }
    }
}
