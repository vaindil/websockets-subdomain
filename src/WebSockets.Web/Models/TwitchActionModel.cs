namespace WebSockets.Web.Models
{
    public class TwitchActionModel
    {
        public string ModUsername { get; set; }

        public string UserUsername { get; set; }

        public string Action { get; set; }

        public int Duration { get; set; }

        public string Reason { get; set; }
    }
}
