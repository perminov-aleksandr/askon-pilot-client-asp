using System;
using System.IO;

namespace Ascon.Pilot.WebClient.ViewModels
{
    /// <summary>
    /// Модель файла
    /// </summary>
    public class FileViewModel
    {
        /// <summary>
        /// Расширение файла.
        /// </summary>
        public string FileExtension
        {
            get { return Path.GetExtension(FileName); }
        }
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Возвращает true ли задданый объект папкой.
        /// </summary>
        public bool IsFolder { get; set; }
        /// <summary>
        /// Возвращает true, если для данного объекта доступен эскиз
        /// </summary>
        public bool IsThumbnailAvailable {
            get
            {
                return FileExtension == ".xps" || FileExtension == ".pdf";
            }
        }

        /// <summary>
        /// Уникальный идентификатор класса-объекта, экземпляром которого является данный файл
        /// </summary>
        public Guid ObjectId { get; set; }
        /// <summary>
        /// Идентификатор класса-объекта, экземпляром является данный файл
        /// </summary>
        public int ObjectTypeId { get; set; }
        /// <summary>
        /// Называние класса-объекта, экземпляром является данный файл
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// Имя класса-объекта, экземпляром является данный файл и его расширение. 
        /// </summary>
        public string Name {
            get
            {
                if (Path.HasExtension(ObjectName) && Path.GetExtension(ObjectName) == FileExtension)
                    return ObjectName;
                return $"{ObjectName}{FileExtension}";
            }
        }
        /// <summary>
        /// Имя данного файла
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Размер данного файла
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Дата последнего изменения файла.
        /// </summary>
        public DateTime LastModifiedDate { get; set; }
        /// <summary>
        /// Дата создания файла
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// Количество дочерних элементов.
        /// </summary>
        public int ChildrenCount { get; set; }
    }
}