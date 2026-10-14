using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
            using (var stream = File.Create(_cacheFile))
            {
                new BinaryFormatter().Serialize(stream, new List<Product>(products));
            }
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

            using (var stream = File.OpenRead(_cacheFile))
            {
                return (List<Product>)new BinaryFormatter().Deserialize(stream);
            }
        }
    }
}
