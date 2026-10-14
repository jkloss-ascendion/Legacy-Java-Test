using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LegacyInventory.Core.Models;

namespace LegacyInventory.Core.Services
{
    public class ProductCache
    {
        private readonly string _cacheFile;
        private readonly TimeSpan _maxAge;

        public ProductCache(string cacheFile, TimeSpan maxAge)
        {
            _cacheFile = cacheFile;
            _maxAge = maxAge;
        }

        public void Save(IList<Product> products)
        {
            var list = new List<Product>(products);
            var json = JsonSerializer.Serialize(list);
            File.WriteAllText(_cacheFile, json);
        }

        public IList<Product> Load()
        {
            if (!File.Exists(_cacheFile))
            {
                return null;
            }

            if (DateTime.UtcNow - File.GetLastWriteTimeUtc(_cacheFile) > _maxAge)
            {
                return null;
            }

            var json = File.ReadAllText(_cacheFile);
            var list = JsonSerializer.Deserialize<List<Product>>(json);
            return list;
        }
    }
}
