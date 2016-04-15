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
            var sidePanel = await RecieveSidePanel();
            //var sidePanel = AltRecieveSidePanel();
            return View(model: sidePanel);
        }

        private string AltRecieveSidePanel()
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
        
        public async Task<string> RecieveSidePanel()
        {
            var serializedData = SerializeObjectsRequest();
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");

            var result = await MakeCall(content);
            DObject[] deserializedResult = JsonConvert.DeserializeObject<DObject[]>(result);
            return result;
        }

        private async Task<string> MakeCall(StringContent content)
        {
            var baseAddress = new Uri(ApplicationConst.PilotServerUrl);
            using (var handler = new HttpClientHandler {CookieContainer = User.GetCookieContainer(baseAddress)})
            using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
            {
                var response = await client.PostAsync(PilotMethod.WEB_CALL, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Server connection failed with status: {response.StatusCode}");
                }
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        private static string SerializeObjectsRequest()
        {
            var getObjectsRequest = new GetObjectsRequest
            {
                api = ApplicationConst.PilotServerApiName,
                method = ApiMethod.GetObjects,
                ids = new[] { DObject.RootId }
            };
            return JsonConvert.SerializeObject(getObjectsRequest);
        }
    }
}
