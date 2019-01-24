using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix;
using WebSockets.Data;
using WebSockets.Data.ZubatRequests;
using WebSockets.Web.Auth;
using WebSockets.Web.Models;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Utils;

namespace WebSockets.Web.Controllers
{
    [Route("zubat")]
    public class ZubatController : ControllerBase
    {
        private readonly ZubatConfig _zubatConfig;
        private readonly VbContext _context;
        private readonly HttpClient _twitchOAuthClient;
        private readonly Helix _twitchApi;

        public ZubatController(IOptions<ZubatConfig> zubatOptions, VbContext context,
            IHttpClientFactory httpClientFactory, Helix twitchApi)
        {
            _zubatConfig = zubatOptions.Value;
            _context = context;
            _twitchOAuthClient = httpClientFactory.CreateClient(NamedHttpClients.TwitchOAuth);
            _twitchApi = twitchApi;
        }

        [HttpGet("requests/count")]
        [MiddlewareFilter(typeof(ZubatJwtAuthMiddleware))]
        public async Task<IActionResult> GetRequestCount([FromQuery]string username = null)
        {
            var twitchId = HttpContext.Items["TwitchId"];
            int count;

            if (username == null)
            {
                var user = await _context.RequestUsers.FindAsync(HttpContext.Items["TwitchId"]);
                if (user != null)
                    count = user.RequestCount;
                else
                    count = 0;
            }
            else
            {
                if (!_zubatConfig.RequestAdminTwitchIds.Contains(twitchId))
                {
                    return Forbid();
                }
                else
                {
                    var userResponse = await _twitchApi.Users.GetUsersAsync(logins: new List<string> { username });
                    if (userResponse?.Users.Length > 0)
                    {
                        var user = await _context.RequestUsers.FindAsync(userResponse.Users[0].Id);
                        if (user != null)
                            count = user.RequestCount;
                        else
                            count = 0;
                    }
                    else
                    {
                        return BadRequest("Twitch user does not exist");
                    }
                }
            }

            return Ok(count);
        }

        [HttpPost("requests")]
        [MiddlewareFilter(typeof(ZubatJwtAuthMiddleware))]
        public async Task<IActionResult> CreateRequest([FromBody]ZubatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Character))
            {
                if (request.IsKillerRequest)
                    return BadRequest("Killer must be selected");
                else
                    return BadRequest("Survivor must be selected");
            }

            if (string.IsNullOrWhiteSpace(request.RequestText))
                return BadRequest("Request text cannot be empty");

            if (request.IsKillerRequest)
            {
                var killerRequest = new KillerRequest
                {
                    KillerName = request.Character,
                    RequestText = request.RequestText,
                    RequestedByTwitchId = (string)HttpContext.Items["TwitchId"],
                    RequestedAt = DateTime.UtcNow
                };

                _context.KillerRequests.Add(killerRequest);
                await _context.SaveChangesAsync();

                return Ok(killerRequest);
            }
            else
            {
                var survivorRequest = new SurvivorRequest
                {
                    SurvivorName = request.Character,
                    RequestText = request.RequestText,
                    RequestedByTwitchId = (string)HttpContext.Items["TwitchId"],
                    RequestedAt = DateTime.UtcNow
                };

                _context.SurvivorRequests.Add(survivorRequest);
                await _context.SaveChangesAsync();

                return Ok(survivorRequest);
            }
        }

        [HttpPut("requests/{id}/completed")]
        [MiddlewareFilter(typeof(ZubatJwtAuthMiddleware))]
        public async Task<IActionResult> CompleteRequest(int id, [FromQuery]bool isKillerRequest)
        {
            var twitchId = HttpContext.Items["TwitchId"];
            if (!_zubatConfig.RequestAdminTwitchIds.Contains(twitchId))
                return Forbid();

            string requestedById = null;

            if (isKillerRequest)
            {
                var killerRequest = await _context.KillerRequests.FindAsync(id);
                if (killerRequest != null)
                {
                    requestedById = killerRequest.RequestedByTwitchId;

                    killerRequest.CompletedAt = DateTime.UtcNow;
                    _context.KillerRequests.Update(killerRequest);
                }
            }
            else
            {
                var survivorRequest = await _context.SurvivorRequests.FindAsync(id);
                if (survivorRequest != null)
                {
                    requestedById = survivorRequest.RequestedByTwitchId;

                    survivorRequest.CompletedAt = DateTime.UtcNow;
                    _context.SurvivorRequests.Update(survivorRequest);
                }
            }

            if (requestedById != null)
            {
                var requestedBy = await _context.RequestUsers.FindAsync(requestedById);
                if (requestedBy != null)
                {
                    requestedBy.RequestCount--;
                    _context.RequestUsers.Update(requestedBy);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("requests/{username}/{count}")]
        [MiddlewareFilter(typeof(ZubatJwtAuthMiddleware))]
        public async Task<IActionResult> AddRequests(string username, int count)
        {
            var twitchId = HttpContext.Items["TwitchId"];
            if (!_zubatConfig.RequestAdminTwitchIds.Contains(twitchId))
                return Forbid();

            var userResponse = await _twitchApi.Users.GetUsersAsync(logins: new List<string> { username });
            if (userResponse.Users.Length == 0)
                return BadRequest("Twitch user does not exist");

            var twitchUser = userResponse.Users[0];
            var user = await _context.RequestUsers.FindAsync(twitchUser.Id);
            if (user == null)
            {
                user = new RequestUser
                {
                    TwitchId = twitchUser.Id,
                    LastUsername = twitchUser.DisplayName,
                    RequestCount = count
                };

                _context.RequestUsers.Add(user);
            }
            else
            {
                user.RequestCount += count;
                _context.RequestUsers.Update(user);
            }

            await _context.SaveChangesAsync();

            return Ok(user.RequestCount);
        }

        [HttpGet("twitch")]
        public async Task<IActionResult> HandleAuthCode([FromQuery]string code)
        {
            var oauthUrl = QueryHelpers.AddQueryString("token", new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", _zubatConfig.TwitchClientId },
                { "client_secret", _zubatConfig.TwitchClientSecret },
                { "redirect_uri", _zubatConfig.TwitchRedirectUri },
                { "grant_type", "authorization_code" }
            });

            var oauthResponse = await _twitchOAuthClient.PostAsync(oauthUrl, null);
            if (!oauthResponse.IsSuccessStatusCode)
                return BadRequest();

            var responseToken = JsonConvert.DeserializeObject<TwitchIdResponse>(await oauthResponse.Content.ReadAsStringAsync());
            var payloadSection = responseToken.IdToken.Split('.')[1];
            var idTokenRawPayload = payloadSection.PadRight(payloadSection.Length + ((4 - (payloadSection.Length % 4)) % 4), '=');
            var idTokenBytes = Convert.FromBase64String(idTokenRawPayload);
            var idToken = JsonConvert.DeserializeObject<TwitchIdToken>(Encoding.UTF8.GetString(idTokenBytes));

            var usernameResponse = await _twitchApi.Users.GetUsersAsync(ids: new List<string> { idToken.Sub });
            if (usernameResponse.Users.Length != 1)
                return BadRequest();

            var username = usernameResponse.Users[0].DisplayName;
            var jwtPayload = new Dictionary<string, object>
            {
                { "twitch_id", idToken.Sub },
                { "username", username }
            };

            var key = Encoding.UTF8.GetBytes(_zubatConfig.JwtSigningKey);
            var token = Jose.JWT.Encode(jwtPayload, key, Jose.JwsAlgorithm.HS256);

            return Redirect($"{_zubatConfig.FrontEndUrl}/auth/{token}");
        }

        [HttpPut("vote/{emoteName}/{voteFor}")]
        public async Task<IActionResult> SubmitVote(string emoteName, string voteFor)
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
                VoteFor = voteFor
            };

            _context.EmoteVotes.Add(vote);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
