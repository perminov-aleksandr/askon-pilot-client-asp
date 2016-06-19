using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ILogger<FilesController> _logger;

        public AccountController(ILogger<FilesController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult LogIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var logInViewModel = new LogInViewModel
            {
#if (DEBUG)
                //DatabaseName = "3d-storage_ru",
                //Login = "admin",
                //Password = "123456"
                DatabaseName = "pilot-ice_ru",
                Login = "sedov",
                Password = "GM9d3Lqw"
#endif
            };
            return View(logInViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInViewModel model)
        {
            if (!ModelState.IsValid)
                return View("LogIn");

            var client = HttpContext.Session.GetClient();
            if (client == null)
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);

            var serverUrl = ApplicationConst.PilotServerUrl;
            try
            {
                client.Connect(serverUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(1, "Не удалось подключиться к серверу", ex);
                ModelState.AddModelError("", "Сервер недоступен.");
                return View();
            }

            var serviceCallbackProxy = CallbackFactory.Get<IServerCallback>();
            var serverApi = client.GetServerApi(serviceCallbackProxy);

            var protectedPassword = model.Password.EncryptAes();
            var useWindowsAuth = model.Login.Contains('\\') || model.Login.Contains('/');
            var dbInfo = serverApi.OpenDatabase(model.DatabaseName, model.Login, protectedPassword, useWindowsAuth);
            if (dbInfo == null)
            {
                ModelState.AddModelError("", "Авторизация не удалась, проверьте данные и повторите вход");
                return View("LogIn", model);
            }
            await SignInAsync(dbInfo);

            DMetadata dMetadata = serverApi.GetMetadata(dbInfo.MetadataVersion);
            HttpContext.Session.SetSessionValues(SessionKeys.DatabaseName, model.DatabaseName);
            HttpContext.Session.SetSessionValues(SessionKeys.Login, model.Login);
            HttpContext.Session.SetSessionValues(SessionKeys.ProtectedPassword, protectedPassword);
            HttpContext.Session.SetSessionValues(SessionKeys.MetaTypes, dMetadata.Types.ToDictionary(x => x.Id, y => y));

            return RedirectToAction("Index", "Home");
        }
        
        private async Task SignInAsync(DDatabaseInfo dbInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbInfo.Person.Login),
                new Claim(ClaimTypes.GivenName, dbInfo.Person.DisplayName),
                new Claim(ClaimTypes.Role, dbInfo.Person.IsAdmin ? Roles.Admin : Roles.User),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ApplicationConst.PilotMiddlewareInstanceName));
            
            await HttpContext.Authentication.SignInAsync(ApplicationConst.PilotMiddlewareInstanceName, principal);
        }
        
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
            HttpContext.Session.GetClient()?.Dispose();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
