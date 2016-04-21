using System;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public async Task<IActionResult> Index()
        {
            //var sidePanel = await SidePanelAsync();
            return View();
        }

        private async Task<string> SidePanelAsync()
        {
            var client = HttpContext.Session.GetClient();
            var content = new GetObjectsRequest {ids = new[] {DObject.RootId}}.ToString();
            var result = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(content));
            if (result.IsSuccessStatusCode)
                return await result.Content.ReadAsStringAsync();
            throw new Exception("Server call failed");
        }

        private string AltSidePanel()
        {
            using (var client = new HttpPilotClient())
            {
                var serverApi = client.GetServerApi(CallbackFactory.Get<IServerCallback>());

                var objects = serverApi.GetObjects(new[] { DObject.RootId });
                return string.Join("<br>", objects.SelectMany(x => new [] {x.Id.ToString(), x.TypeId.ToString()}));
            }
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
