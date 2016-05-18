using Ascon.Pilot.WebClient.Extensions;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Types()
        {
            return View(HttpContext.Session.GetMetatypes().Values);
        }

        public IActionResult GetTypeIcon(int id)
        {
            const string svgContentType = "image/svg+xml";
            var mTypes = HttpContext.Session.GetMetatypes();
            if (mTypes.ContainsKey(id))
            {
                var mType = mTypes[id];
                if (mType.Icon != null)
                    return File(mType.Icon, svgContentType);
            }
            return File(Url.Content("~/images/file.svg"), svgContentType);
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
