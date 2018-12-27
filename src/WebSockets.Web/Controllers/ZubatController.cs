using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebSockets.Data;

namespace WebSockets.Web.Controllers
{
    [Route("zubat")]
    public class ZubatController : ControllerBase
    {
        private readonly VbContext _context;

        public ZubatController(VbContext context)
        {
            _context = context;
        }

        [HttpPut("vote/{emoteName}/{isVoteForNew}")]
        public async Task<IActionResult> SubmitVote(string emoteName, bool isVoteForNew)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            var vote = await _context.EmoteVotes.FirstOrDefaultAsync(x => x.IpAddress == ipAddress && x.EmoteName == emoteName);
            if (vote != null)
                return NoContent();

            vote = new EmoteVote
            {
                IpAddress = ipAddress,
                LoggedAt = DateTime.UtcNow,
                EmoteName = emoteName,
                IsVoteForNew = isVoteForNew
            };

            _context.EmoteVotes.Add(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
