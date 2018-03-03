using System.Threading.Tasks;

namespace WebSockets.Data
{
    public interface IRepository
    {
        Task<KeyValue> GetByKeyAsync(string key);
        Task<KeyValue> CreateOrUpdateAsync(string key, string value);
        Task DeleteByKeyAsync(string key);
    }
}
