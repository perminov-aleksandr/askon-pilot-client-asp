using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index(Guid? id)
        {
            var model = new UserPositionViewModel
            {
                Path = id.HasValue ? GetPath(id.Value) : new List<string> { "Root" },
                SidePanel = new SidePanelViewModel
                {
                    ObjectId = id ?? DObject.RootId
                }
            };
            return View(model);
        }

        private List<string> GetPath(Guid id)
        {
            return new List<string> {"Root"};
        }

        public IActionResult SidePanel(Guid? id)
        {
            return ViewComponent("SidePanel", id);
        }

        public async Task<IActionResult> GetNodeChilds(Guid id)
        {
            var client = HttpContext.Session.GetClient();
            var request = new GetObjectsRequest{ids = new [] {id}};
            var result = await request.SendAsync(client);
            var childsIds = result[0].Children.Select(x => x.ObjectId).ToArray();
            request = new GetObjectsRequest{ids = childsIds};
            result = await request.SendAsync(client);
            var childNodes = result.Select(x => new
            {
                id = x.Id,
                text = x.Id
            }).ToArray();
            return Json(childNodes);
        }

        public IActionResult GetObject(Guid? id)
        {
            return ViewComponent("FilesPanel", id);
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
