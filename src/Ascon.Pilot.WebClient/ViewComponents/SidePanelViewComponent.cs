using System;
using System.Collections.Generic;
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
using ProtoBuf;

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
            var objects = await GetObjectsAsync(client, new[] {id.Value});
            var childrens = await GetObjectsAsync(client, objects[0].Children.Select(x => x.ObjectId).ToArray());

            var metadataVersion = GetMetadataVersion();
            var metadata = await new GetMetadataRequest
            {
                localVersion = metadataVersion
            }.SendAsync(client);

            var mTypes = metadata?.Types.ToDictionary(x => x.Id, y => y);
            var model = new SidePanelViewModel
            {
                ObjectId = id.Value,
                Types = mTypes,
                Items = childrens? //.Where(x => mTypes[x.TypeId].IsProjectFolder())
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
                                : childs 
                                    //.Where(obj => mTypes[obj.TypeId].IsProjectFolder())
                                    .Select(z => GetItemsWithChilds(z, client).Result).ToList()
                        };
                    })
                    .ToList()
            };

            if (id.Value == DObject.RootId)
                return View(model);

            var prevId = id.Value;
            var parentId = objects[0].ParentId;
            while (parentId != Guid.Empty)
            {
                var parentObject = await GetObjectsAsync(client, new[] {parentId});
                var parentChilds =
                    await GetObjectsAsync(client, parentObject[0].Children.Select(x => x.ObjectId).ToArray());
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
                var dbInfo = Serializer.Deserialize<DDatabaseInfo>(ms);
                metadataVersion = dbInfo.MetadataVersion;
            }
            return metadataVersion;
        }

        private static async Task<SidePanelItem> GetItemsWithChilds(DObject obj, HttpClient client)
        {
            var sidePanelItem = new SidePanelItem {
                DObject = obj
            };

            if (obj.Children == null || obj.Children.Any())
                return sidePanelItem;

            var childrenIds = obj.Children.Select(x => x.ObjectId).ToArray();
            var childrens = await GetObjectsAsync(client, childrenIds);
            
            sidePanelItem.SubItems = childrens?.Select(x => new SidePanelItem
            {
                DObject = x
            }).ToList();
            return sidePanelItem;
        }

        private static async Task<DObject[]> GetObjectsAsync(HttpClient client, Guid[] ids)
        {
            if (ids == null || !ids.Any())
                return null;

            var getObjectsRequest = new GetObjectsRequest {ids = ids};
            return await getObjectsRequest.SendAsync(client);
        }
    }
}
