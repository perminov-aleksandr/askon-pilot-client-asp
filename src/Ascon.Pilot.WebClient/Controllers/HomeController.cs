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
            const string pngContentType = "image/png";
            var mTypes = HttpContext.Session.GetMetatypes();
            if (mTypes.ContainsKey(id))
            {
                var mType = mTypes[id];
                if (mType.Icon != null)
                    return File(mType.Icon, "image/svg+xml");
            }
            return File(Url.Content("~/images/file.png"), pngContentType);
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
