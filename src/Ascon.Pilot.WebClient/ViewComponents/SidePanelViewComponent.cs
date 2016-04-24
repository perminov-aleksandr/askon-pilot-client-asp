using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public async Task<IViewComponentResult> InvokeAsync(Guid? id)
        {
            return await GetSidePanelAsync(id);
        }
        
        public async Task<IViewComponentResult> GetSidePanelAsync(Guid? id)
        {
            var client = HttpContext.Session.GetClient();
            DObject[] objects = await GetObjectsAsync(client, new []{ id ?? DObject.RootId});
            var childrens = await GetObjectsAsync(client, objects[0].Children.Select(x => x.ObjectId).ToArray());
            var model = new SidePanelViewModel {
                ObjectId = id ?? DObject.RootId,
                Items = childrens?.Select(x =>
                {
                    var childs = GetObjectsAsync(client, x.Children?.Select(y => y.ObjectId).ToArray()).Result;
                    return new SidePanelItem
                    {
                        DObject = x,
                        SubItems = childs?.Select(z => new SidePanelItem { DObject = z }).ToList()
                    };
                }).ToList()
            };

            if (!id.HasValue || id.Value == DObject.RootId)
                return View(model);

            var prevId = id.Value;
            Guid parentId = objects[0].ParentId;
            while (parentId != DObject.RootId)
            {
                var parentObject = await GetObjectsAsync(client, new[] { parentId });
                var parentChilds = await GetObjectsAsync(client, parentObject[0].Children.Select(x => x.ObjectId).ToArray());
                model.Items = new List<SidePanelItem>(parentChilds.Select(x => new SidePanelItem {
                    DObject = x,
                    SubItems = x.Id == prevId ? model.Items : new List<SidePanelItem>()
                }).ToArray());
                prevId = parentId;
                parentId = parentObject[0].ParentId;
            }
            return View(model);
        }

        public async Task<SidePanelItem> GetItemsWithChilds(DObject obj)
        {
            var childrens = await GetObjectsAsync(HttpContext.Session.GetClient(), obj.Children.Select(x => x.ObjectId).ToArray());
            return new SidePanelItem
            {
                DObject = obj,
                SubItems = childrens.Select(x => new SidePanelItem
                {
                    DObject = x,
                    SubItems = new List<SidePanelItem>()
                }).ToList()
            };
        }

        private static async Task<DObject[]> GetObjectsAsync(HttpClient client, Guid[] ids)
        {
            var content = new GetObjectsRequest { ids = ids }.ToString();
            var result = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(content));
            if (!result.IsSuccessStatusCode)
            {
                result = await client.PostAsync(PilotMethod.WEB_CONNECT, new StringContent(""));
                if (!result.IsSuccessStatusCode)
                    throw new Exception("Server call failed while call GetObjects");
            }

            var stringResult = await result.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<DObject[]>(stringResult);
            }
            catch (JsonReaderException ex)
            {
                return null;
            }
        }
    }
}
