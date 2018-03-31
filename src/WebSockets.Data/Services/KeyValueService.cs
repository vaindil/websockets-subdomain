using System.Threading.Tasks;

namespace WebSockets.Data.Services
{
    public class KeyValueService
    {
        readonly VbContext _context;

        public KeyValueService(VbContext context)
        {
            _context = context;
        }

        public Task<KeyValue> GetByKeyAsync(string key)
        {
            return _context.KeyValues.FindAsync(key);
        }

        public async Task CreateOrUpdateAsync(string key, string value)
        {
            var kv = await _context.KeyValues.FindAsync(key);
            if (kv == null)
                _context.KeyValues.Add(new KeyValue(key, value));
            else
                kv.Value = value;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteByKeyAsync(string key)
        {
            var kv = await _context.KeyValues.FindAsync(key);
            if (kv != null)
            {
                _context.KeyValues.Remove(kv);
                await _context.SaveChangesAsync();
            }
        }
    }
}
