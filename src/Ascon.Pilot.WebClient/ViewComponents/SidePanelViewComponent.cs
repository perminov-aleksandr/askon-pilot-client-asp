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
            //id = id ?? DObject.RootId;

            id = DObject.RootId;

            var serverApi = HttpContext.Session.GetServerApi();
            var rootObject = serverApi.GetObjects(new[] { id.Value }).First();

            var mTypes = HttpContext.Session.GetMetatypes();
            var model = new SidePanelViewModel
            {
                ObjectId = id.Value,
                Types = mTypes,
                Items = new List<SidePanelItem>()
            };

            var rootChildrenIds = rootObject.Children?
                //.Where(x => mTypes[x.TypeId].Children?.Any() == true)
                .Select(x => x.ObjectId).ToArray();
            var rootChildrens = serverApi.GetObjects(rootChildrenIds);
            if (rootChildrens == null)
                return View(model);

            foreach (var rootChildren in rootChildrens)
            {
                var hasChildrens = rootChildren.Children?.Any(x => mTypes[x.TypeId].Children?.Any() == true) == true;
                var sidePanelItem = new SidePanelItem
                {
                    DObject = rootChildren,
                    Type = mTypes[rootChildren.TypeId],
                    SubItems = hasChildrens ? new List<SidePanelItem>() : null
                };

                /*if (hasChildrens)
                {
                    var childIds = rootChildren.Children
                        //.Where(x => mTypes[x.TypeId].Children?.Any() == true)
                        .Select(y => y.ObjectId).ToArray();
                    var childs = serverApi.GetObjects(childIds);

                    foreach (var child in childs)
                    {
                        var panelItem = GetItemsWithChilds(child, mTypes, serverApi);
                        sidePanelItem.SubItems.Add(panelItem);
                    }
                }*/
                model.Items.Add(sidePanelItem);
            }

            /*if (id.Value == DObject.RootId)
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
                        .Select(x => new SidePanelItem
                        {
                            Type = mTypes[x.TypeId],
                            DObject = x,
                            SubItems = x.Id == prevId ? subtree : null
                        }).ToList();
                }
                prevId = parentId;
                parentId = parentObject.ParentId;
            }*/
            return View(model);
        }
    }
}
