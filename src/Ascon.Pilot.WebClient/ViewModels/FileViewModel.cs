using System;
using System.IO;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class FileViewModel
    {
        public Guid Id { get; set; }
        public bool IsFolder { get; set; }
        public string Extension {
            get { return Path.GetExtension(FileName); }
        }  
        public bool IsThumbnailAvailable {
            get
            {
                var extension = Extension;
                return extension == ".xps" || extension == ".pdf";
            }
        }

        public int ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }

        public Guid ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string Name {
            get
            {
                var extension = Extension;
                if (Path.HasExtension(ObjectName) && Path.GetExtension(ObjectName) == extension)
                    return ObjectName;
                return $"{ObjectName}{extension}";
            }
        }
        public string FileName { get; set; }

        public int Size { get; set; }
        private string _sizeStr = string.Empty;
        public string SizeString
        {
            get
            {
                if (string.IsNullOrEmpty(_sizeStr))
                {
                    string[] sizes = { "b", "Kb", "Mb", "Gb" };
                    double len = Size;
                    int order = 0;
                    while (len >= 1024 && order + 1 < sizes.Length)
                    {
                        order++;
                        len = len / 1024;
                    }
                    _sizeStr = $"{len:0.##} {sizes[order]}";
                }
                return _sizeStr;
            }
        }

        public DateTime LastModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ChildrenCount { get; set; }
    }
}