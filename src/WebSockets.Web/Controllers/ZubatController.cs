using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _accessor;

        public ZubatController(VbContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        [HttpPut("vote/{emoteName}/{voteFor}")]
        public async Task<IActionResult> SubmitVote(string emoteName, string voteFor)
        {
            var ipAddress = _accessor.HttpContext.Connection.RemoteIpAddress;
            var vote = await _context.EmoteVotes.FirstOrDefaultAsync(x => x.IpAddress == ipAddress && x.EmoteName == emoteName);
            if (vote != null)
                return NoContent();

            vote = new EmoteVote
            {
                IpAddress = ipAddress,
                LoggedAt = DateTime.UtcNow,
                EmoteName = emoteName,
                VoteFor = voteFor
            };

            _context.EmoteVotes.Add(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
