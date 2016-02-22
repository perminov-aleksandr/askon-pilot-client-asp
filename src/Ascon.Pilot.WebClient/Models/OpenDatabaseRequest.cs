namespace Ascon.Pilot.WebClient.Models
{
    public class OpenDatabaseRequest
    {
        public int licenseType { get; set; }
        public string api { get; set; }
        public string method { get; set; }
        public bool useWindowsAuth { get; set; }
        public string database { get; set; }
        public string login { get; set; }
        public string protectedPassword { get; set; }
    }
}