using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace WebSockets.Data
{
    public class Repository : IRepository
    {
        private readonly string _connectionString;

        public Repository(IConfiguration config)
        {
            _connectionString = config.GetValue<string>("PostgresConnectionString");
        }

        internal IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public async Task<KeyValue> GetByKeyAsync(string key)
        {
            using (var db = Connection)
            {
                db.Open();
                return await db.QueryFirstOrDefaultAsync<KeyValue>(
                    "SELECT key, value FROM key_value WHERE key = @key", new { key });
            }
        }

        public async Task<KeyValue> CreateOrUpdateAsync(KeyValue kv)
        {
            using (var db = Connection)
            {
                db.Open();
                await db.ExecuteAsync(
                    "INSERT INTO key_value (key, value) " +
                    "VALUES (@key, @value) " +
                    "ON CONFLICT (key) DO UPDATE " +
                        "SET value = @value",
                    new { key = kv.Key, value = kv.Value });
            }

            return kv;
        }

        public async Task DeleteByKeyAsync(string key)
        {
            using (var db = Connection)
            {
                db.Open();
                await db.ExecuteAsync("DELETE FROM key_value WHERE key = @key", new { key });
            }
        }
    }
}
