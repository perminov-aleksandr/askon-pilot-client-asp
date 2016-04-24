using System;
using System.Collections.Generic;
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
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AltLogIn(LogInViewModel model)
        {
            if (!ModelState.IsValid)
                return View("LogIn");
            
            var serverUrl = ApplicationConst.PilotServerUrl;
            var connectionCredentials = ConnectionCredentials.GetConnectionCredentials(serverUrl, model.Login, model.Password.ConvertToSecureString());
            using (var client = new HttpPilotClient(connectionCredentials, new JsonMarshallingFactory()))
            {
                var serviceCallbackProxy = new Castle.DynamicProxy.ProxyGenerator().CreateInterfaceProxyWithoutTarget<IServerCallback>();
                var serverApi = client.GetServerApi(serviceCallbackProxy);
                var dbInfo = serverApi.OpenDatabase(model.DatabaseName, model.Login, model.Password.EncryptAes(), false);
                if (dbInfo == null)
                {
                    ModelState.AddModelError("", "Авторизация не удалась, проверьте данные и повторите вход");
                    return View("LogIn", model);
                }

                await SignIn(dbInfo, model.Password, "");
                
                var objects = serverApi.GetObjects(new[] { DObject.RootId });
                if (objects != null && objects.Any())
                {
                    var childrenCount = objects[0].Children.Count;
                }

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AltAltLogIn(LogInViewModel model, string returnUrl = null)
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

            var serializedData = SerializeOpenDatabaseRequestData(model);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            var result = await client.PostAsync(PilotMethod.WEB_CALL, content);
            var resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                var dbInfo = JsonConvert.DeserializeObject<DDatabaseInfo>(resultContent);
                await SignIn(dbInfo, model.Password, "");

                return RedirectToAction("Index", "Home");
            }
            catch (JsonReaderException)
            {
                var databaseNotFoundMessage = $"Database [{model.DatabaseName}] not found";
                if (resultContent == databaseNotFoundMessage)
                {
                    ModelState.AddModelError("", "Указанное название базы данных не существует");
                }
                else
                {
                    ModelState.AddModelError("", "Указанные имя пользователи или пароль неверны. Проверьте данные и попробуйте еще раз");
                }
                return View("LogIn", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var baseAddress = new Uri(ApplicationConst.PilotServerUrl);
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    var serializedData = SerializeOpenDatabaseRequestData(model);
                    var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(PilotMethod.WEB_CALL, content);
                    var resultContent = await result.Content.ReadAsStringAsync();

                    try
                    {
                        var dbInfo = JsonConvert.DeserializeObject<DDatabaseInfo>(resultContent);
                        var cookieString = cookieContainer.GetCookieHeader(baseAddress);
                        await SignIn(dbInfo, model.Password, cookieString);

                        return RedirectToAction("Index", "Home");
                    }
                    catch (JsonReaderException)
                    {
                        var databaseNotFoundMessage = $"Database [{model.DatabaseName}] not found";
                        if (resultContent == databaseNotFoundMessage)
                        {
                            ModelState.AddModelError("", "Указанное название базы данных не существует");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Указанные имя пользователи или пароль неверны. Проверьте данные и попробуйте еще раз");
                        }
                        return View(model);
                    }
                }
            }
            return View(model);
        }

        private async Task SignIn(DDatabaseInfo dbInfo, string pwd, string sid)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbInfo.Person.Login),
                new Claim(ClaimTypes.GivenName, dbInfo.Person.DisplayName),
                new Claim(ClaimTypes.Role, dbInfo.Person.IsAdmin ? Roles.Admin : Roles.User),
                new Claim(ClaimTypes.Sid, sid),
                new Claim("PWD", pwd),
                new Claim("DatabaseId", dbInfo.DatabaseId.ToString())
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, ApplicationConst.PilotMiddlewareInstanceName));
            await HttpContext.Authentication.SignInAsync(ApplicationConst.PilotMiddlewareInstanceName, principal);
        }

        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
            HttpContext.Session.GetClient().Dispose();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        private static string SerializeOpenDatabaseRequestData(LogInViewModel model)
        {
            var openDatabaseRequest = new OpenDatabaseRequest
            {
                licenseType = 100,
                api = ApplicationConst.PilotServerApiName,
                method = ApiMethod.OpenDatabase,
                useWindowsAuth = false,
                database = model.DatabaseName,
                login = model.Login,
                protectedPassword = model.Password
            };
            var serializedRequest = JsonConvert.SerializeObject(openDatabaseRequest);
            return serializedRequest;
        }
    }
}
