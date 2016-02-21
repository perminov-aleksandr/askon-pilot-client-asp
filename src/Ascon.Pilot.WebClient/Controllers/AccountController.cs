using System.Net;
using System.Threading.Tasks;
using Ascon.Pilot.WebClient.ServerManager;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Ascon.Pilot.WebClient.Controllers
{
    public class AccountController : Controller
    {
        // GET: /<controller>/
        public IActionResult LogIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var response = await ServerRequestManager.MakeOpenDatabaseRequest(model.DatabaseName, model.Login, model.Password);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (returnUrl == null)
                        return RedirectToAction("Index", "Home");
                    return Redirect(returnUrl);
                }
                ModelState.AddModelError("result", "Данные для входа неверны. Проверьте данные и попробуйте еще раз");
            }
            return View(model);
        }

        [Authorize]
        public IActionResult LogOff()
        {
            return View();
        }
    }
}
