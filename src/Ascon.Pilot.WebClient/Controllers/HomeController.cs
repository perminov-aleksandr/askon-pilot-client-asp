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
        
        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
