namespace Ascon.Pilot.WebClient.ServerManager
{
    public class OpenDatabaseRequest
    {
        public string api { get; set; }
        public string method { get; set; }
        public string database { get; set; }
        public string login { get; set; }
        public string protectedPassword { get; set; }
        public bool useWindowsAuth { get; set; }
        public int licenseType { get; set; }
    }
}