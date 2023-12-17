using System.Collections.Generic;
using System.Net.Http;
namespace NLBE_Bot {
    public class ApiRequester {
        public static string GetRequest(string url, Dictionary<string,string> parameters = null)
        {
            using (var client = new HttpClient())
            {
                // Set the API key header
                if (parameters != null){
                    foreach (var parameter in parameters){
                        client.DefaultRequestHeaders.Add(parameter.Key, parameter.Value);
                    }
                }
                var response = client.GetAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        } 
    }
}
