namespace Ascon.Pilot.WebClient.Models.Requests
{
    public abstract class Request
    {
        public string api { get; set; }
        public string method { get; set; }
    }
}