using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewComponents;
using Ascon.Pilot.WebClient.ViewModels;
using Castle.Core.Logging;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Session;
using Microsoft.Extensions.Logging;
#if DNX451
using MuPDFLib;
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace Ascon.Pilot.WebClient.Controllers
{
    public enum FilesPanelType
    {
        Grid,
        List
    }

    [Authorize]
    public class FilesController : Controller
    {
        private ILogger<FilesController> _logger;

        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(Guid? id)
        {
            id = id ?? DObject.RootId;
            var model = new UserPositionViewModel
            {
                CurrentFolderId = id.Value,
                FilesPanelType = ApplicationConst.DefaultFilesPanelType
            };
            return View(model);
        }
        
        public async Task<IActionResult> GetNodeChilds(Guid id)
        {
            return await Task.Run(() =>
            {
                var serverApi = HttpContext.Session.GetServerApi();
                var node = serverApi.GetObjects(new[] {id}).First();

                var types = HttpContext.Session.GetMetatypes();

                var childIds = node.Children?
                                    .Where(x => types[x.TypeId].Children?.Any() == true)
                                    .Select(child => child.ObjectId).ToArray();
                var nodeChilds = serverApi.GetObjects(childIds);
                
                var childNodes = nodeChilds
                    .Select(x =>
                    {
                        var mType = types[x.TypeId];
                        var sidePanelItem = new SidePanelItem
                        {
                            DObject = x,
                            Type = mType,
                            SubItems = x.Children.Any(y => types[y.TypeId].Children.Any()) ? new List<SidePanelItem>() : null
                        };
                        return sidePanelItem.GetDynamic(id, types);
                    })
                    .ToArray();
                return Json(childNodes);
            });
        }

        public IActionResult SidePanel(Guid? id)
        {
            return ViewComponent(typeof (SidePanelViewComponent), id);
        }

        public IActionResult GetObject(Guid id, FilesPanelType panelType = ApplicationConst.DefaultFilesPanelType)
        {
            return ViewComponent(typeof (FilesPanelViewComponent), id, panelType);
        }

        public IActionResult Preview(Guid id, int size, string name)
        {
            ViewBag.Url = Url.Action("DownloadPdf", new { id, size, name });
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax) return PartialView();
            return View();
        }

        public IActionResult DownloadPdf(Guid id, int size, string name)
        {
            var serverApi = HttpContext.Session.GetServerApi();
            var fileChunk = serverApi.GetFileChunk(id, 0, size);
            var fileDownloadName = string.IsNullOrWhiteSpace(name) ? id.ToString() : name;
            if (Response.Headers.ContainsKey("Content-Disposition"))
                Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", $"inline; filename={fileDownloadName}");
            return new FileContentResult(fileChunk, "application/pdf");
        }

        public async Task<IActionResult> Download(Guid id, int size, string name)
        {
            return await Task.Run(() =>
            {
                {
                    var serverApi = HttpContext.Session.GetServerApi();
                    var fileChunk = serverApi.GetFileChunk(id, 0, size);
                    return new FileContentResult(fileChunk, "application/octet-stream")
                    {
                        FileDownloadName = string.IsNullOrWhiteSpace(name) ? id.ToString() : name
                    };
                }
            });
        }

        public async Task<IActionResult> DownloadArchive(Guid[] objectsIds)
        {
            if (objectsIds.Length == 0)
                return HttpNotFound();

            var serverApi = HttpContext.Session.GetServerApi();
            var objects = serverApi.GetObjects(objectsIds);

            using (var compressedFileStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, true))
                {
                    foreach (var obj in objects)
                    {
                        var dFile = obj.ActualFileSnapshot.Files.First();
                        var fileId = dFile.Body.Id;
                        var fileSize = dFile.Body.Size;
                        var fileBody = serverApi.GetFileChunk(fileId, 0, (int)fileSize);

                        var zipEntry = zipArchive.CreateEntry(dFile.Name, CompressionLevel.NoCompression);

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(fileBody))
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            await originalFileStream.CopyToAsync(zipEntryStream);
                        }
                    }
                }

                return new FileContentResult(compressedFileStream.ToArray(), "application/zip") { FileDownloadName = "archive.zip" };
            }
        }

        public async Task<IActionResult> Thumbnail(Guid id, int size, string extension)
        {
            const string pngContentType = "image/png";
            const string svgContentType = "image/svg+xml";
            var virtualFileResult = File(Url.Content("~/images/file.svg"), svgContentType);
#if DNX451
            if (size >= 10*1024*1024)
                return virtualFileResult;

            var serverApi = HttpContext.Session.GetServerApi();
            var file = serverApi.GetFileChunk(id, 0, size);
            try
            {
                if (file != null)
                {
                    int page = 1;
                    int dpi = 150;
                    RenderType RenderType = RenderType.RGB;
                    bool rotateAuto = false;
                    string password = "";

                    var fileName = $"tmp/{id}{extension}";
                    using (var fileStream = System.IO.File.Create(fileName))
                        fileStream.Write(file, 0, file.Length);

                    byte[] thumbnailContent;
                    using (MuPDF pdfDoc = new MuPDF(fileName, password))
                    {
                        pdfDoc.Page = page;
                        Bitmap bm = pdfDoc.GetBitmap(0, 0, dpi, dpi, 0, RenderType, rotateAuto, false, 0);
                        HttpContext.Response.ContentType = pngContentType;
                        using (var ms = new MemoryStream())
                        {
                            bm.Save(ms, ImageFormat.Png);
                            thumbnailContent = ms.ToArray();
                        }
                    }

                    System.IO.File.Delete(fileName);

                    return File(thumbnailContent, pngContentType, $"{id}.png");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(1, "Unable to generate thumbnail for file", ex);
            }
#endif
            return virtualFileResult;
        }

        private byte[] GetFileFromObject(Guid id)
        {
            var serverApi = HttpContext.Session.GetServerApi();
            var dObjects = serverApi.GetObjects(new [] {id});
            var obj = dObjects.First();
            if (obj?.ActualFileSnapshot?.Files?.Any() == false)
                return null;

            var file = obj.ActualFileSnapshot.Files.First();
            var fileExtension = Path.GetExtension(file.Name);
            if (fileExtension == ".pdf" || fileExtension == ".xps")
            {
                byte[] result = serverApi.GetFileChunk(file.Body.Id, 0, (int)file.Body.Size);
                return result;
            }
            return null;
        }
    }
}