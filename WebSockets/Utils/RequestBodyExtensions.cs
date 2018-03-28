using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebSockets.Web.Utils
{
    public static class RequestBodyExtensions
    {
        /// <summary>
        /// Retrieves the raw request body as a string from the Request.Body stream.
        /// </summary>
        /// <seealso cref="https://weblog.west-wind.com/posts/2017/Sep/14/Accepting-Raw-Request-Body-Content-in-ASPNET-Core-API-Controllers" />
        /// <param name="request">HttpRequest to get the body of</param>
        /// <param name="encoding">Optional: encoding, defaults to UTF8</param>
        /// <returns>Request body as a string</returns>
        public static async Task<string> GetBodyAsStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (var reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Retrieves the raw request body as a byte array from the Request.Body stream.
        /// </summary>
        /// <seealso cref="https://weblog.west-wind.com/posts/2017/Sep/14/Accepting-Raw-Request-Body-Content-in-ASPNET-Core-API-Controllers" />
        /// <param name="request">HttpRequest to get the body of</param>
        /// <returns>Request body as a byte array</returns>
        public static async Task<byte[]> GetBodyAsBytesAsync(this HttpRequest request)
        {
            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
