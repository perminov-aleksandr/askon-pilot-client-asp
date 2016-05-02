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
            return new List<string> {"Root"};
        }

        public async Task<IActionResult> GetNodeChilds(Guid id)
        {
            return await Task.Run(() =>
            {
                var serverApi = HttpContext.Session.GetServerApi();
                var node = serverApi.GetObjects(new[] {id});

                var childIds = node.First().Children.Select(child => child.ObjectId).ToArray();
                var nodeChilds = serverApi.GetObjects(childIds);

                var types = HttpContext.Session.GetMetatypes();

                var childNodes = nodeChilds
                    .Where(x => types[x.TypeId].IsProjectFolder())
                    .Select(x =>
                    {
                        var mType = types[x.TypeId];
                        var icon = ApplicationConst.TypesGlyphiconDictionary.ContainsKey(mType.Name)
                            ? $"glyphicon glyphicon-{ApplicationConst.TypesGlyphiconDictionary}"
                            : "";
                        return new
                        {
                            id = x.Id,
                            text = x.GetTitle(mType),
                            icon,
                            nodes = x.Children.Any(y => types[y.TypeId].IsProjectFolder()) ? new dynamic[] {} : null
                        };
                    })
                    .ToArray();
                return Json(childNodes);
            });
        }

        public IActionResult SidePanel(Guid? id)
        {
            return ViewComponent(typeof (SidePanelViewComponent), id);
        }

        public IActionResult GetObject(Guid? id)
        {
            return ViewComponent(typeof (FilesPanelViewComponent), id);
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
    }
}