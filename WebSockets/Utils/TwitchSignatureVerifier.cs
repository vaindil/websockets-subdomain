using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebSockets.Web.Utils
{
    public static class TwitchSignatureVerifier
    {
        public static bool Verify(string secret, string signature, byte[] body)
        {
            using (var alg = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hexSignature = BytesToHex(alg.ComputeHash(body)).ToLowerInvariant();
                return hexSignature == signature;
            }
        }

        private static string BytesToHex(IEnumerable<byte> input)
        {
            return string.Concat(input.Select(x => x.ToString("X2")).ToArray());
        }
    }
}
