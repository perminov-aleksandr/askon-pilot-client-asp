using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
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
            return new List<string> { "Root" };
        }

        public async Task<IActionResult> GetNodeChilds(Guid id)
        {
            var client = HttpContext.Session.GetClient();
            var request = new GetObjectsRequest { ids = new[] { id } };
            var result = await request.SendAsync(client);

            var childsIds = result[0].Children.Select(x => x.ObjectId).ToArray();
            request = new GetObjectsRequest { ids = childsIds };
            result = await request.SendAsync(client);

            var types = HttpContext.Session.GetMetatypes();

            var childNodes = result
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
                        nodes = x.Children.Any(y => types[y.TypeId].IsProjectFolder()) ? new dynamic[]{} : null
                    };
                })
                .ToArray();
            return Json(childNodes);
        }

        public IActionResult SidePanel(Guid? id)
        {
            return ViewComponent(typeof(SidePanelViewComponent), id);
        }

        public IActionResult GetObject(Guid? id)
        {
            return ViewComponent(typeof(FilesPanelViewComponent), id);
        }
        
        public async Task<IActionResult> Download(Guid id, long size, string name)
        {
            var client = HttpContext.Session.GetClient();
            var request = new GetFileChunkRequest { id = id, count = size };
            var response = await request.SendAsync(client);
            return new FileContentResult(response, "application/octet-stream")
            {
                FileDownloadName = string.IsNullOrWhiteSpace(name) ? id.ToString() : name
            };
        }
    }
}
