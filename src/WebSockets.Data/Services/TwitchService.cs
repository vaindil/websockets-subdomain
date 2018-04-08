using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Data.Twitch;

namespace WebSockets.Data.Services
{
    public class TwitchService
    {
        private readonly WsContext _context;

        public TwitchService(WsContext context)
        {
            _context = context;
        }

        public Task<TwitchMarker> GetMarkerByIdAsync(int id)
        {
            return _context.TwitchMarkers
                .Include(m => m.Stream)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public Task<TwitchStream> GetStreamAndMarkersByStreamIdAsync(string id)
        {
            return _context.TwitchStreams
                .Include(s => s.Markers)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<TwitchStream> GetStreamAndMarkersByVodIdAsync(string id)
        {
            return _context.TwitchStreams
                .Include(s => s.Markers)
                .FirstOrDefaultAsync(s => s.VodId == id);
        }

        public async Task<TwitchStream> CreateStreamAsync(TwitchStream stream)
        {
            _context.TwitchStreams.Add(stream);
            await _context.SaveChangesAsync();

            return stream;
        }

        public async Task<TwitchMarker> CreateMarkerAsync(TwitchMarker marker)
        {
            _context.TwitchMarkers.Add(marker);
            await _context.SaveChangesAsync();

            return marker;
        }

        public async Task DeleteMarkerByIdAsync(int id)
        {
            var marker = await _context.TwitchMarkers.FindAsync(id);
            if (marker != null)
            {
                _context.TwitchMarkers.Remove(marker);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStreamByStreamIdAsync(string id)
        {
            var stream = await _context.TwitchStreams.FindAsync(id);
            if (stream != null)
            {
                _context.TwitchStreams.Remove(stream);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStreamByVodIdAsync(string id)
        {
            var stream = await _context.TwitchStreams.FirstOrDefaultAsync(s => s.VodId == id);
            if (stream != null)
            {
                _context.TwitchStreams.Remove(stream);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStreamAsync(TwitchStream stream)
        {
            _context.TwitchStreams.Update(stream);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMarkerAsync(TwitchMarker marker)
        {
            _context.TwitchMarkers.Update(marker);
            await _context.SaveChangesAsync();
        }
    }
}
