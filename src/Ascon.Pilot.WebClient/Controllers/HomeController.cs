using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewComponents;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const string RootBreadcrumb = "Root";

        public async Task<ViewResult> Index(Guid? id)
        {
            var model = new UserPositionViewModel
            {
                Path = id.HasValue ? GetPath(id.Value) : new List<string> { RootBreadcrumb },
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
            var client = HttpContext.Session.GetClient();
            var request = new GetObjectsRequest { ids = new[] { id } };
            var result = await request.SendAsync(client);
            var childsIds = result[0].Children.Select(x => x.ObjectId).ToArray();
            request = new GetObjectsRequest { ids = childsIds };
            result = await request.SendAsync(client);
            var childNodes = result.Select(x => new
            {
                id = x.Id,
                text = x.Id
            }).ToArray();
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

        public async Task<IActionResult> Types()
        {
            var client = HttpContext.Session.GetClient();
            long localMetadata;
            using (var ms = new MemoryStream(HttpContext.Session.Get(SessionKeys.DBInfo)))
            {
                var dbInfo = ProtoBuf.Serializer.Deserialize<DDatabaseInfo>(ms);
                localMetadata = dbInfo.MetadataVersion;
            }
            var getMetadataRequest = new GetMetadataRequest {localVersion = localMetadata};
            var response = await getMetadataRequest.SendAsync(client);
            return View(response.Types);
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
