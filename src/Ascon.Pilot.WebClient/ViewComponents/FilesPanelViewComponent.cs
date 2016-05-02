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
                    var serverApi = HttpContext.Session.GetServerApi();
                    var folder = serverApi.GetObjects(new[] { folderId }).First();

                    if (folder.Children == null || !folder.Children.Any())
                        return View(new FileViewModel[] { });

                    var childrenIds = folder.Children.Select(x => x.ObjectId).ToArray();
                    var childrens = serverApi.GetObjects(childrenIds);
                    var types = HttpContext.Session.GetMetatypes();
                    var model = childrens
                        .Where(x =>
                        {
                            var mType = types[x.TypeId];
                            return mType.IsProjectFile();
                        })
                        .Select(dFile =>
                        {
                            var file = dFile.Files.FirstOrDefault();
                            if (file == null) return null;
                            return new FileViewModel
                            {
                                Id = file.Body.Id,
                                LastModifiedDate = file.Body.Modified,
                                Name = dFile.GetTitle(types[dFile.TypeId]),
                                Size = file.Body.Size
                            };
                        });
                    return View(model); 
                }
            });
        }
    }
}