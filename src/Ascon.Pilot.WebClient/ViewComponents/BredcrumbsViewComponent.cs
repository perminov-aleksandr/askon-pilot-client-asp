using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class BreadcrumbsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid id)
        {
            var types = HttpContext.Session.GetMetatypes();

            var serverApi = HttpContext.GetServerApi();
            Queue<KeyValuePair<string, string>> result = new Queue<KeyValuePair<string, string>>();
            Guid parentId = id;
            bool isSource = ViewBag.IsSource ?? false;
            while (parentId != DObject.RootId)
            {
                var obj = serverApi.GetObjects(new[] { parentId })[0];
                var mType = types[obj.TypeId];
                result.Enqueue(new KeyValuePair<string, string>(Url.Action("Index", "Files", new { id = obj.Id, isSource }), obj.GetTitle(mType)));
                if (mType.IsMountable)
                    isSource = false;
                parentId = obj.ParentId;
            }
            result.Enqueue(new KeyValuePair<string, string>(Url.Action("Index", "Files", new { id = DObject.RootId }), "Начало"));
            return View(result);
        }
    }
}