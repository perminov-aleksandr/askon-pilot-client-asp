namespace Ascon.Pilot.WebClient.Models
{
    public abstract class Request
    {
        public string api { get; set; }
        public string method { get; set; }
    }

    public class OpenDatabaseRequest : Request
    {
        public int licenseType { get; set; }
        public bool useWindowsAuth { get; set; }
        public string database { get; set; }
        public string login { get; set; }
        public string protectedPassword { get; set; }
    }
}