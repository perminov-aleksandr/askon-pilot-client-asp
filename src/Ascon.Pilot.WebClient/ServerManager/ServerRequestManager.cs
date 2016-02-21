using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.ServerManager
{
    public class ServerRequestManager
    {
        public static async Task<HttpResponseMessage> MakeOpenDatabaseRequest(string database, string login, string password)
        {
            var openDatabaseRequest = new OpenDatabaseRequest
            {
                licenseType = 100,
                api = "IServerApi",
                method = "OpenDatabase",
                useWindowsAuth = false,
                database = database,
                login = login,
                protectedPassword = password
            };
            var serializedRequest = JsonConvert.SerializeObject(openDatabaseRequest);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident / 6.0)");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4,de;q=0.2");
                
                var requestUri = ApplicationConst.PilotServerUrl + "/web/call";
                HttpContent content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                using (var response = await client.PostAsync(requestUri, content))
                //using (var content = response.Content)
                {
                    return response;
                }
            }
        }
    }
}