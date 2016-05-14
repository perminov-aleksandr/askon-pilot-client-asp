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
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

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
        public IActionResult Index(Guid? id)
        {
            var model = new UserPositionViewModel
            {
                CurrentFolderId = id ?? DObject.RootId,
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

                        var zipEntry = zipArchive.CreateEntry(dFile.Name);

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

        public IActionResult Thumbnail(Guid id)
        {
            //todo: generate thumbnail and send it
            var virtualFileResult = File(Url.Content("~/images/file.png"), "image/png");
            return virtualFileResult;
        }
    }
}