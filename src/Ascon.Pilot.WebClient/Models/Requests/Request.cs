using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public abstract class Request<TResult>
    {
        public string api { get; set; }
        public string method { get; set; }

        public Request(string api, string method)
        {
            this.api = api;
            this.method = method;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public async Task<TResult> SendAsync(HttpClient client)
        {
            var serializedResult = await PostAsync(client);
            try
            {
                var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult);
                return deserializedResult;
            }
            catch (JsonReaderException ex)
            {
                return default(TResult);
            }
        }

        protected async Task<string> PostAsync(HttpClient client)
        {
            var content = new StringContent(ToString());
            var response = await client.PostAsync(PilotMethod.WEB_CALL, content);
            var serializedResult = await response.Content.ReadAsStringAsync();
            return serializedResult;
        }
    }
}