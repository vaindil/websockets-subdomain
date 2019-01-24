namespace WebSockets.Web.Models.Configs
{
    public class ZubatConfig
    {
        public string FrontEndUrl { get; set; }

        public string ApiSecret { get; set; }

        public string JwtSigningKey { get; set; }

        public string TwitchClientId { get; set; }

        public string TwitchClientSecret { get; set; }

        public string TwitchRedirectUri { get; set; }

        public string[] RequestAdminTwitchIds { get; set; }
    }
}
