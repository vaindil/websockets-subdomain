using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace WebSockets.Web.Data
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
                return await db.QueryFirstOrDefaultAsync<KeyValue>(
                    "SELECT \"key\", value FROM key_value WHERE \"key\" = @key", new { key });
            }
        }

        public async Task<KeyValue> CreateOrUpdateAsync(string key, string value)
        {
            using (var db = Connection)
            {
                await db.ExecuteAsync(
                    "INSERT INTO key_value (\"key\", value) " +
                    "VALUES (@key, @value) " +
                    "ON CONFLICT (\"key\") DO UPDATE " +
                        "SET value = @value",
                    new { key, value });
            }

            return new KeyValue(key, value);
        }

        public async Task DeleteByKeyAsync(string key)
        {
            using (var db = Connection)
            {
                await db.ExecuteAsync("DELETE FROM key_value WHERE \"key\" = @key", new { key });
            }
        }
    }
}
