using System.Threading.Tasks;

namespace WebSockets.Data
{
    public interface IRepository
    {
        Task<KeyValue> GetByKeyAsync(string key);
        Task<KeyValue> CreateOrUpdateAsync(KeyValue kv);
        Task DeleteByKeyAsync(string key);
    }
}
