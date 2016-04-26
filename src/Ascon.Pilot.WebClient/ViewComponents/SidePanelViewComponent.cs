using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Http;
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
            id = id ?? DObject.RootId;
            var client = HttpContext.Session.GetClient();
            DObject[] objects = await GetObjectsAsync(client, new []{ id.Value });
            var childrens = await GetObjectsAsync(client, objects[0].Children.Select(x => x.ObjectId).ToArray());

            var metadataVersion = GetMetadataVersion();
            DMetadata metadata = await new GetMetadataRequest
            {
                localVersion = metadataVersion
            }.SendAsync(client);

            var mTypes = metadata?.Types.ToDictionary(x => x.Id, y => y);
            Debug.Assert(mTypes != null, $"{nameof(mTypes)} != null");
            var model = new SidePanelViewModel {
                ObjectId = id.Value,
                Types = mTypes,
                Items = childrens?//.Where(x => mTypes[x.TypeId].IsProjectFolder())
                        .Select(x =>
                        {
                            var childIds = x.Children?.Select(y => y.ObjectId).ToArray();
                            var childs = GetObjectsAsync(client, childIds).Result;
                            return new SidePanelItem
                            {
                                Type = mTypes[x.TypeId],
                                DObject = x,
                                SubItems = childs == null || childs.Length == 0 
                                           ? (x.Children != null && x.Children.Any() ? new List<SidePanelItem>() : null)
                                           : childs//.Where(obj => mTypes[obj.TypeId].IsProjectFolder())
                                                    .Select(z => GetItemsWithChilds(z, client).Result).ToList()
                            };
                        })
                        .ToList()
            };

            if (id.Value == DObject.RootId)
                return View(model);

            var prevId = id.Value;
            Guid parentId = objects[0].ParentId;
            while (parentId != Guid.Empty)
            {
                var parentObject = await GetObjectsAsync(client, new[] { parentId });
                var parentChilds = await GetObjectsAsync(client, parentObject[0].Children.Select(x => x.ObjectId).ToArray());
                if (parentChilds.Length != 0)
                {
                    var subtree = model.Items;
                    model.Items = parentChilds
                        //.Where(x => mTypes[x.TypeId].IsProjectFolder())
                        .Select(x => new SidePanelItem
                        {
                            DObject = x,
                            SubItems = x.Id == prevId ? subtree : null
                        }).ToList();
                }
                prevId = parentId;
                parentId = parentObject[0].ParentId;
            }
            return View(model);
        }

        private long GetMetadataVersion()
        {
            long metadataVersion;
            using (var ms = new MemoryStream(HttpContext.Session.Get(SessionKeys.DBInfo)))
            {
                var dbInfo = ProtoBuf.Serializer.Deserialize<DDatabaseInfo>(ms);
                metadataVersion = dbInfo.MetadataVersion;
            }
            return metadataVersion;
        }

        private async Task<SidePanelItem> GetItemsWithChilds(DObject obj, HttpClient client)
        {
            var childrens = await GetObjectsAsync(client, obj.Children.Select(x => x.ObjectId).ToArray());
            return new SidePanelItem
            {
                DObject = obj,
                SubItems = childrens?.Select(x => new SidePanelItem
                {
                    DObject = x
                }).ToList()
            };
        }

        private static async Task<DObject[]> GetObjectsAsync(HttpClient client, Guid[] ids)
        {
            var content = new GetObjectsRequest { ids = ids }.ToString();
            var result = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(content));
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Server call failed while call GetObjects");
            }

            var stringResult = await result.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<DObject[]>(stringResult);
            }
            catch (JsonReaderException ex)
            {
                return new DObject[] {};
            }
        }
    }
}
