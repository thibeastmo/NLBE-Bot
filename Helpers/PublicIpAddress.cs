using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLBE_Bot.Helpers
{
    internal class PublicIpAddress
    {
        #pragma warning disable S1075 // URIs should not be hardcod
        private const string ApiUrl = "https://api.ipify.org?format=json";
        #pragma warning restore S1075 // URIs should not be hardcoded

        public static async Task<string> GetPublicIpAddressAsync()
        {
            using HttpClient client = new();

            try
            {
                string response = await client.GetStringAsync(ApiUrl);
                var json = JsonSerializer.Deserialize<IpResponse>(response);
                return json.ip;
            }
            catch (Exception ex)
            {                
                return string.Format("Unable to retrieve IP, cause: {0}", ex);
            }
        }

        public class IpResponse
        {
            public string ip { get; set; }
        }
    }
}
