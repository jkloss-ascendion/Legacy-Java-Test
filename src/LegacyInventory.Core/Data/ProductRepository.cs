using System;
using System.Collections.Generic;
using System.Data;
using LegacyInventory.Core.Models;
using Microsoft.Data.SqlClient;

namespace LegacyInventory.Core.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<Product> GetAll()
        {
            var products = new List<Product>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT Id, Sku, Name, UnitPrice, QuantityOnHand, LastUpdated FROM dbo.Products", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(Map(reader));
                    }
                }
            }

            return products;
        }

        public Product GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT Id, Sku, Name, UnitPrice, QuantityOnHand, LastUpdated FROM dbo.Products WHERE Id = @Id", connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public void UpdateQuantity(int id, int quantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("UPDATE dbo.Products SET QuantityOnHand = @Qty, LastUpdated = GETUTCDATE() WHERE Id = @Id", connection))
            {
                command.Parameters.Add("@Qty", SqlDbType.Int).Value = quantity;
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static Product Map(IDataRecord record)
        {
            return new Product
            {
                Id = record.GetInt32(0),
                Sku = record.GetString(1),
                Name = record.GetString(2),
                UnitPrice = record.GetDecimal(3),
                QuantityOnHand = record.GetInt32(4),
                LastUpdated = record.GetDateTime(5)
            };
        }
    }
}
