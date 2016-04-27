using System;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class FileViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public long Size { get; set; }
        //public string Icon { get; set; } 
    }
}