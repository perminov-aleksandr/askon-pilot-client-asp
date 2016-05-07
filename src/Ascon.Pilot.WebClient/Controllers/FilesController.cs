using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewComponents;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    public enum FilesPanelType
    {
        Grid,
        List
    }

    public class FilesController : Controller
    {
        public IActionResult Index(Guid? id)
        {
            var model = new UserPositionViewModel
            {
                Path = id.HasValue ? GetPath(id.Value) : new List<string>(),
                SidePanel = new SidePanelViewModel
                {
                    ObjectId = id ?? DObject.RootId
                }
            };
            return View(model);
        }

        private List<string> GetPath(Guid id)
        {
            //todo: finish
            return new List<string> {"..."};
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

        public IActionResult GetObject(Guid id, FilesPanelType panelType = FilesPanelType.List)
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

        public IActionResult Thumbnail(Guid id)
        {
            //todo: generate thumbnail and send it
            var virtualFileResult = File(Url.Content("~/images/file.png"), "image/png");
            return virtualFileResult;
        }
    }
}