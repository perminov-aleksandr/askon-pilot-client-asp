using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    public class FilesController : Controller
    {
        public async Task<IActionResult> Download(Guid id, long size)
        {
            var client = HttpContext.Session.GetClient();
            var request = new GetFileChunkRequest{ id = id, count = size};
            var response = await client.PostAsync(PilotMethod.WEB_CALL, new StringContent(request.ToString()));
            var result = await response.Content.ReadAsByteArrayAsync();
            return new FileContentResult(result, response.Content.Headers.ContentType.MediaType);
        }
    }
}
