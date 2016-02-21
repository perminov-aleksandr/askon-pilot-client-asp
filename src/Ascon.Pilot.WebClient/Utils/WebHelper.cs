using System.Net.Http;
using System.Threading.Tasks;

namespace Ascon.Pilot.WebClient.Utils
{
    public static class WebHelper
    {
        public static async Task<string> MakePostRequest(string url, string postContent)
        {
            using (var client = new HttpClient())
            using (var response = await client.PostAsync(url, new StringContent(postContent)))
            using (var content = response.Content)
            {
                var result = await content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
