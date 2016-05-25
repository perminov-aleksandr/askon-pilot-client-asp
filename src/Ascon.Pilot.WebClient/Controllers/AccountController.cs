using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    /// <summary>
    /// Контроллер учётных записей.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Процедура авторизации.
        /// </summary>
        /// <param name="returnUrl">Url, на который следует вернуться после успешного взода в систему </param>
        /// <returns>Представление данных модели типа logInViewModel </returns>
        /// <seealso cref="Ascon.Pilot.WebClient.ViewModels"/>
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

        /// <summary>
        /// Процедура авторизации
        /// </summary>
        /// <param name="model"> Модель, описываемая классом logInViewModel</param>
        /// <returns>Перенаправление действия на Index контроллера Home </returns>
        /// <seealso cref="Ascon.Pilot.WebClient.ViewModels"/>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInViewModel model)
        {
            if (!ModelState.IsValid)
                return View("LogIn");

            var client = HttpContext.Session.GetClient();
            var serverUrl = ApplicationConst.PilotServerUrl;
            client.Connect(serverUrl);
            
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
            //HttpContext.Session.SetSessionValues(SessionKeys.DBInfo, dbInfo);
            HttpContext.Session.SetSessionValues(SessionKeys.DatabaseName, model.DatabaseName);
            HttpContext.Session.SetSessionValues(SessionKeys.Login, model.Login);
            HttpContext.Session.SetSessionValues(SessionKeys.ProtectedPassword, protectedPassword);
            HttpContext.Session.SetSessionValues(SessionKeys.MetaTypes, dMetadata.Types.ToDictionary(x => x.Id, y => y));

            return RedirectToAction("Index", "Home");
        }
        
        /// <summary>
        /// Процедура входа в систему
        /// </summary>
        /// <param name="dbInfo">База данных о пользователях системы</param>
        /// <returns>Удостоверение авторизованного в системе пользвоателя.</returns>
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
        
        /// <summary>
        /// Выход из системы
        /// </summary>
        /// <returns>Перенаправление на представление Index контроллера Home</returns>
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
            HttpContext.Session.GetClient().Dispose();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
