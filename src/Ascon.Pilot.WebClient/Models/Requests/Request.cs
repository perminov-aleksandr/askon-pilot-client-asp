using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Models.Requests
{
    public abstract class Request
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
    }
}