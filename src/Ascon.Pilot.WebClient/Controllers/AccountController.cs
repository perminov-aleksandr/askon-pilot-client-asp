using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;
using Ascon.Pilot.WebClient.Extensions;
using Ascon.Pilot.WebClient.Models.Requests;
using Ascon.Pilot.WebClient.ViewModels;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace Ascon.Pilot.WebClient.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult LogIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var logInViewModel = new LogInViewModel
            {
#if (DEBUG)
                DatabaseName = "3d-storage_ru",
                Login = "admin",
                Password = "123456"
#endif
            };
            return View(logInViewModel);
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View("LogIn", model);

            var client = HttpContext.Session.GetClient();
            if (client == null)
            {
                ModelState.AddModelError("", "Не удается подключиться к серверу");
                return View("LogIn", model);
            }
            
            var request = new OpenDatabaseRequest
            {
                database = model.DatabaseName,
                login = model.Login,
                protectedPassword = model.Password
            };
            var dbInfo = await request.SendAsync(client);
            if (dbInfo == null)
            {
                ModelState.AddModelError("", "Не удалось войти. Проверьте введенные данные или попробуйте позже");
                return View("LogIn", model);
            }
            await SignInAsync(dbInfo, model.Password);
            RecieveAndSetMetatypes(client, dbInfo.MetadataVersion);
            return RedirectToAction("Index", "Home");
        }
        
        private async Task SignInAsync(DDatabaseInfo dbInfo, string pwd)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbInfo.Person.Login),
                new Claim(ClaimTypes.GivenName, dbInfo.Person.DisplayName),
                new Claim(ClaimTypes.Role, dbInfo.Person.IsAdmin ? Roles.Admin : Roles.User),
                new Claim("PWD", pwd),
                new Claim("DatabaseId", dbInfo.DatabaseId.ToString())
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ApplicationConst.PilotMiddlewareInstanceName));
            
            await HttpContext.Authentication.SignInAsync(ApplicationConst.PilotMiddlewareInstanceName, principal);

            SetSessionValues(SessionKeys.DBInfo, dbInfo);
        }

        /// <summary>
        /// Set value of type <typeparam name="T">T</typeparam> at session dictionary using protobuf-net
        /// </summary>
        /// <typeparam name="T">type of value to set. Must be proto-serializable</typeparam>
        /// <param name="key">key of value</param>
        /// <param name="value">value to set</param>
        private void SetSessionValues<T>(string key, T value)
        {
            using (var bs = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(bs, value);
                HttpContext.Session.Set(key, bs.ToArray());
            }
        }

        private void RecieveAndSetMetatypes(HttpClient client, long metadataVersion)
        {
            var response = new GetMetadataRequest { localVersion = metadataVersion }.SendAsync(client).Result;
            SetSessionValues(SessionKeys.MetaTypes, response.Types.ToDictionary(x => x.Id, y => y));
        }

        private void AltRecieveAndSetMetatypes(long metadataVersion)
        {
            var client = HttpContext.Session.GetClient();
            var response = new GetMetadataRequest { localVersion = metadataVersion }.SendAsync(client).Result;
            SetSessionValues(SessionKeys.MetaTypes, response.Types.ToDictionary(x => x.Id, y => y));
        }

        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
            HttpContext.Session.GetClient().Dispose();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
