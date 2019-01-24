namespace WebSockets.Web.Models
{
    public class TwitchIdToken
    {
        public string Iss { get; set; }

        public string Sub { get; set; }

        public string Aud { get; set; }

        public string Exp { get; set; }

        public string Iat { get; set; }

        public string Nonce { get; set; }
    }
}
