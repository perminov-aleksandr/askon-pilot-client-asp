using System;
using System.IO;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class FileViewModel
    {
        public Guid Id { get; set; }
        public bool IsFolder { get; set; }
        public string ObjectName { get; set; }
        public string Name {
            get
            {
                if (Path.HasExtension(ObjectName) && Path.GetExtension(ObjectName) == Path.GetExtension(FileName))
                    return ObjectName;
                return $"{ObjectName}{Path.GetExtension(FileName)}";
            }
        }
        public string FileName { get; set; }
        public long Size { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        //public string Icon { get; set; } 
    }
}