using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class SidePanelViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid? id)
        {
            return await Task.Run(() => GetSidePanel(id)) ;
        }

        public IViewComponentResult GetSidePanel(Guid? id)
        {
            id = id ?? DObject.RootId;

            var serverApi = HttpContext.Session.GetServerApi();
            var rootObject = serverApi.GetObjects(new[] { id.Value }).First();

            var mTypes = HttpContext.Session.GetMetatypes();
            var sidePanelItems = new List<SidePanelItem>();
            var model = new SidePanelViewModel
            {
                ObjectId = id.Value,
                Types = mTypes,
                Items = sidePanelItems
            };

            var rootChildrenIds = rootObject.Children.Select(x => x.ObjectId).ToArray();
            var childrens = serverApi.GetObjects(rootChildrenIds);
            if (childrens == null)
                return View(model);

            foreach (var children in childrens)
            {
                var sidePanelItem = new SidePanelItem
                {
                    DObject = children,
                    Type = mTypes[children.TypeId],
                    SubItems = children.Children != null && children.Children.Any() ? new List<SidePanelItem>() : null
                };

                if (children.Children != null && children.Children.Any())
                {
                    var childIds = children.Children?.Select(y => y.ObjectId).ToArray();
                    var childs = serverApi.GetObjects(childIds);

                    foreach (var child in childs)
                    {
                        var panelItem = GetItemsWithChilds(child, mTypes, serverApi);
                        sidePanelItem.SubItems.Add(panelItem);
                    }
                }
                sidePanelItems.Add(sidePanelItem);
            }

            if (id.Value == DObject.RootId)
                return View(model);

            var prevId = id.Value;
            var parentId = rootObject.ParentId;
            while (parentId != Guid.Empty)
            {
                var parentObject = serverApi.GetObjects(new[] { parentId }).First();
                var parentChilds = serverApi.GetObjects(parentObject.Children.Select(x => x.ObjectId).ToArray());
                if (parentChilds.Count != 0)
                {
                    var subtree = model.Items;
                    model.Items = parentChilds
                        //.Where(x => mTypes[x.TypeId].IsProjectFolder())
                        .Select(x => new SidePanelItem
                        {
                            Type = mTypes[x.TypeId],
                            DObject = x,
                            SubItems = x.Id == prevId ? subtree : null
                        }).ToList();
                }
                prevId = parentId;
                parentId = parentObject.ParentId;
            }
            return View(model);
        }
        
        private static SidePanelItem GetItemsWithChilds(DObject obj, IDictionary<int, MType> types, IServerApi serverApi)
        {
            var sidePanelItem = new SidePanelItem
            {
                DObject = obj,
                Type = types[obj.TypeId]
            };

            if (obj.Children == null || obj.Children.Any())
                return sidePanelItem;

            var childrenIds = obj.Children.Select(x => x.ObjectId).ToArray();
            var childrens = serverApi.GetObjects(childrenIds);

            sidePanelItem.SubItems = childrens?.Select(x => new SidePanelItem
            {
                DObject = x,
                Type = types[x.TypeId],
                SubItems = x.Children.Any(y => types[y.TypeId].IsProjectFolder()) ? new List<SidePanelItem>() : null
            }).ToList();
            return sidePanelItem;
        }
    }
}
