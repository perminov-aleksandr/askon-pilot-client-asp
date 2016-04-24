using System;
using System.Linq;
using System.Threading.Tasks;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.ViewComponents
{
    public class FilesPanelViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid folderId)
        {
            var client = HttpContext.Session.GetClient();
            var request = new GetObjectsRequest {
                ids = new []{folderId}
            };
            var folder = (await request.SendAsync(client))[0];
            var model = folder.Files.Select(dFile => new FileViewModel
            {
                Id = dFile.Body.Id,
                Name = dFile.Name,
                Size = dFile.Body.Size
            }).ToList();
            return View(model);
        }
    }
}