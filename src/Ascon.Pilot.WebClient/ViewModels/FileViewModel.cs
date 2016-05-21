using System;
using System.IO;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class FileViewModel
    {
        public string FileExtension
        {
            get { return Path.GetExtension(FileName); }
        }
        
        public Guid Id { get; set; }
        public bool IsFolder { get; set; }
        public bool IsThumbnailAvailable {
            get
            {
                return FileExtension == ".xps" || FileExtension == ".pdf";
            }
        }
        public Guid ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string ObjectName { get; set; }
        public string Name {
            get
            {
                if (Path.HasExtension(ObjectName) && Path.GetExtension(ObjectName) == FileExtension)
                    return ObjectName;
                return $"{ObjectName}{FileExtension}";
            }
        }
        public string FileName { get; set; }
        public int Size { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ChildrenCount { get; set; }
    }
}