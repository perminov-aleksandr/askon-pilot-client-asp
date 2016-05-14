using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Controllers;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class FilesPanelViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid folderId, FilesPanelType panelType)
        {
            return await Task.Run(() =>
            {
                {
                    var viewName = panelType == FilesPanelType.List ? "List" : "Grid";

                    var types = HttpContext.Session.GetMetatypes();
                    var serverApi = HttpContext.Session.GetServerApi();
                    var folder = serverApi.GetObjects(new[] { folderId }).First();
                    
                    if (folder.Children?.Any(/*x => types[x.TypeId].Children?.Any() == false*/) != true)
                        return View(viewName, new FileViewModel[] { });

                    var childrenIds = folder.Children
                        //.Where(x => types[x.TypeId].Children?.Any() == false)
                        .Select(x => x.ObjectId).ToArray();
                    var childrens = serverApi.GetObjects(childrenIds);
                    var model = new List<FileViewModel>(childrens.Count);
                    foreach (var dObject in childrens/*.Where(x => x.ActualFileSnapshot.Files.Any())*/)
                    {
                        var mType = types[dObject.TypeId];
                        if (mType.Children.Any())
                            model.Add(new FileViewModel
                            {
                                Id = dObject.Id,
                                IsFolder = true,
                                ObjectName = dObject.GetTitle(mType),
                                FileName = dObject.GetTitle(mType),
                                CreatedDate = dObject.Created,
                                LastModifiedDate = dObject.Created,
                                ChildrenCount = dObject.Children.Count(x => types[x.TypeId].Children.Any())
                            });
                        else if (dObject.ActualFileSnapshot?.Files?.Any() == true)
                        {
                            var file = dObject.ActualFileSnapshot.Files.First();
                            model.Add(new FileViewModel
                            {
                                Id = file.Body.Id,
                                IsFolder = false,
                                ObjectId = dObject.Id,
                                ObjectName = dObject.GetTitle(mType),
                                FileName = file.Name,
                                Size = file.Body.Size,
                                LastModifiedDate = file.Body.Modified,
                                CreatedDate = file.Body.Created
                            });
                        }
                    };
                    return View(viewName, model); 
                }
            });
        }
    }
}