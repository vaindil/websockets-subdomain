using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Utils;

namespace WebSockets.Web.Controllers
{
    [Route("twitch")]
    public class TwitchController : ControllerBase
    {
        private readonly TwitchOAuthGeneratorConfig _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public TwitchController(
            IOptions<TwitchOAuthGeneratorConfig> options,
            IHttpClientFactory clientFactory,
            ILogger<TwitchController> logger)
        {
            _config = options.Value;
            _httpClient = clientFactory.CreateClient(NamedHttpClients.TwitchOAuth);
            _logger = logger;
        }

        [HttpGet("oauth")]
        public async Task<IActionResult> OAuthResponse([FromQuery]string code)
        {
            if (Request.Query.TryGetValue("error", out var error))
            {
                Request.Query.TryGetValue("error_description", out var errorDesc);
                _logger.LogError($"Error from Twitch: {error} | {errorDesc}");

                return BadRequest("An error occurred while getting the token, sorry!");
            }

            var url = $"token?client_id={_config.ClientId}&client_secret={_config.ClientSecret}&code={code}" +
                $"&grant_type=authorization_code&redirect_uri={_config.RedirectUri}";

            var response = await _httpClient.PostAsync(url, null);
            var respBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error getting Twitch token: {respBody}");
                return BadRequest("An error occurred while getting the token, sorry!");
            }

            _logger.LogInformation($"Token generated: {respBody}");

            return Ok("Token was generated, thanks!");
        }
    }
}
