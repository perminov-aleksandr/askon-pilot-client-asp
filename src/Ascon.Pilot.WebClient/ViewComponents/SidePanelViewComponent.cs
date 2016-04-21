using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class SidePanelViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = HttpContext.Session.GetClient();
            var content = new GetObjectsRequest { ids = new[] { DObject.RootId } }.ToString();
            var result = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(content));
            if (result.IsSuccessStatusCode)
            {
                var stringResult = await result.Content.ReadAsStringAsync();
                DObject[] objects = JsonConvert.DeserializeObject<DObject[]>(stringResult);
                return View(new SidePanelViewModel {
                    Items = objects
                });
            }
            throw new Exception("Server call failed");
            //var getObjectsRequest = new GetObjectsRequest
            //{
            //    ids = new[] { DObject.RootId }
            //};
            //var serializedRequest = getObjectsRequest.ToString();
            //var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            
            //var result = await MakeCall(content);
            //DObject[] objects = JsonConvert.DeserializeObject<DObject[]>(result);
            //return View(new SidePanelViewModel
            //{
            //    Items = objects
            //});
        }
        
        private async Task<string> MakeCall(StringContent content)
        {
            var client = HttpContext.Session.GetClient();
            if (client == null)
                throw new WebException("Unable to connect to server");
            var response = await client.PostAsync(PilotMethod.WEB_CALL, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Server call request failed with status: {response.StatusCode}");
            }
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
