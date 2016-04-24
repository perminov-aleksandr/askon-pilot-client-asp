using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class GetObjectsRequest : Request
    {
        public GetObjectsRequest() 
            : base(ApplicationConst.PilotServerApiName, ApiMethod.GetObjects)
        {
        }
        
        public Guid[] ids { get; set; }

        public async Task<DObject[]> SendAsync(HttpClient client)
        {
            if (ids == null || ids.Length == 0)
                throw new Exception("ids must be defined");

            var serializedRequest = ToString();
            var result = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(serializedRequest));
            if (!result.IsSuccessStatusCode)
                throw new Exception("Server call failed");

            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DObject[]>(stringResult);
        }
    }
}