namespace Ascon.Pilot.WebClient.Models.Requests
{
    public class OpenDatabaseRequest : Request
    {
        public int licenseType { get; set; }
        public bool useWindowsAuth { get; set; }
        public string database { get; set; }
        public string login { get; set; }
        public string protectedPassword { get; set; }
    }
}