using System;
using System.Collections.Generic;
using System.IO;
using LegacyInventory.Core.Models;
using LegacyInventory.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LegacyInventory.Tests
{
    [TestClass]
    public class ProductCacheTests
    {
        private string _cacheFile;

        [TestInitialize]
        public void SetUp()
        {
            _cacheFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        }

        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists(_cacheFile))
            {
                File.Delete(_cacheFile);
            }
        }

        [TestMethod]
        public void Load_ReturnsSavedProducts()
        {
            var cache = new ProductCache(_cacheFile, TimeSpan.FromMinutes(5));
            var products = new List<Product>
            {
                new Product { Id = 1, Sku = "SKU-001", Name = "Widget", UnitPrice = 9.99m, QuantityOnHand = 12, LastUpdated = DateTime.UtcNow }
            };

            cache.Save(products);
            var loaded = cache.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual("SKU-001", loaded[0].Sku);
            Assert.AreEqual(9.99m, loaded[0].UnitPrice);
        }

        [TestMethod]
        public void Load_ReturnsNull_WhenCacheFileMissing()
        {
            var cache = new ProductCache(_cacheFile, TimeSpan.FromMinutes(5));

            Assert.IsNull(cache.Load());
        }

        [TestMethod]
        public void Load_ReturnsNull_WhenCacheExpired()
        {
            var cache = new ProductCache(_cacheFile, TimeSpan.Zero);
            cache.Save(new List<Product> { new Product { Id = 2, Sku = "SKU-002", Name = "Gadget" } });
            File.SetLastWriteTimeUtc(_cacheFile, DateTime.UtcNow.AddMinutes(-1));

            Assert.IsNull(cache.Load());
        }
    }
}
