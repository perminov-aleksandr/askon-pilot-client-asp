using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Ascon.Pilot.WebClient.Models;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            //var sidePanel = await RecieveSidePanel();
            return View(/*model: sidePanel*/);
        }
        
        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }

        public async Task<string> RecieveSidePanel()
        {
            var baseAddress = new Uri(ApplicationConst.PilotServerUrl);
            
            var cookieContainer = new CookieContainer();
            if (User.HasClaim(x => x.Type == ClaimTypes.Sid))
            {
                var sidString = User.FindFirst(x => x.Type == ClaimTypes.Sid).Value;
                cookieContainer.SetCookies(baseAddress, sidString);
            }
            else
            {
                return "";
            }

            var serializedData = SerializeObjectsRequest();
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            
            using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
            {
                var response = await client.PostAsync(PilotMethod.WEB_CALL, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(string.Format("Server connection failed with status: {0}", response.StatusCode)); ;
                }
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        private static string SerializeObjectsRequest()
        {
            var getObjectsRequest = new GetObjectsRequest()
            {
                api = ApplicationConst.PilotServerApiName,
                method = ApiMethod.GetObjects,
                ids = new[] {Guid.Parse("00000001-0001-0001-0001-000000000001") }
            };
            return JsonConvert.SerializeObject(getObjectsRequest);
        }
    }
}
