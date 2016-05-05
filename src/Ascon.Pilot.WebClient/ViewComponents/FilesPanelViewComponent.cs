using System;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class FilesPanelViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid folderId)
        {
            return await Task.Run(() =>
            {
                {
                    var types = HttpContext.Session.GetMetatypes();
                    var serverApi = HttpContext.Session.GetServerApi();
                    var folder = serverApi.GetObjects(new[] { folderId }).First();

                    if (folder.Children?.Any(x => types[x.TypeId].Children?.Any() == false) != true)
                        return View(new FileViewModel[] { });

                    var childrenIds = folder.Children
                        .Where(x => types[x.TypeId].Children?.Any() == false)
                        .Select(x => x.ObjectId).ToArray();
                    var childrens = serverApi.GetObjects(childrenIds);
                    var model = childrens
                        .Where(x => x.Files?.Any() == true)
                        .Select(dFile =>
                        {
                            var file = dFile.Files.First();
                            return new FileViewModel
                            {
                                Id = file.Body.Id,
                                Name = dFile.GetTitle(types[dFile.TypeId]),
                                Size = file.Body.Size,
                                LastModifiedDate = file.Body.Modified
                            };
                        });
                    return View(model); 
                }
            });
        }
    }
}