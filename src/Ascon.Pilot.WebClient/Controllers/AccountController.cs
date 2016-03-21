using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Models;
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

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> AltLogIn(LogInViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var message = new MarshallingMessage()
        //        {
        //            Interface = ApplicationConst.PilotServerApiName,
        //            Method = ApiMethod.OpenDatabase
        //        };
        //        var baseAddress = new Uri(ApplicationConst.PilotServerUrl);
        //        using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
        //        using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
        //        {
        //            using (var ms = new MemoryStream())
        //            {
        //                ProtoBuf.Serializer.Serialize(ms, message);
        //                using (var content = new ByteArrayContent(ms.ToArray()))
        //                {
        //                    var task = await client.PostAsync(PilotMethod.CALL, content);
        //                    await ProcessResponse(task);
        //                }
        //            }
        //        }
        //    }
        //    return View("LogIn");
        //}

        //private async Task<IActionResult> ProcessResponse(HttpResponseMessage task)
        //{
        //    var serializedResult = await task.Content.ReadAsByteArrayAsync();
        //    using (var ms = new MemoryStream(serializedResult))
        //    {
        //        var result = ProtoBuf.Serializer.Deserialize<MarshallingMessage>(ms);
        //    }
        //    return View();
        //}

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
                    HttpResponseMessage result;
                    try
                    {
                        result = await client.PostAsync(PilotMethod.WEB_CONNECT, new StringContent(string.Empty));
                        if (!result.IsSuccessStatusCode)
                            throw new Exception(string.Format("Server connection failed with status: {0}", result.StatusCode));
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Не удается подключиться к серверу");
                        return View(model);
                    }

                    var serializedData = SerializeOpenDatabaseRequestData(model);
                    var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
                    result = await client.PostAsync(PilotMethod.WEB_CALL, content);
                    var resultContent = await result.Content.ReadAsStringAsync();

                    try
                    {
                        var dbInfo = JsonConvert.DeserializeObject<DDatabaseInfo>(resultContent);

                        var cookieString = cookieContainer.GetCookieHeader(baseAddress);
                        cookieString = cookieContainer.GetCookies(baseAddress)["SID"].ToString();

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, dbInfo.Person.Login),
                            new Claim(ClaimTypes.GivenName, dbInfo.Person.DisplayName),
                            new Claim(ClaimTypes.Role, dbInfo.Person.IsAdmin ? Roles.Admin : Roles.User),
                            new Claim(ClaimTypes.Sid, cookieString)
                        };

                        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Custom"));
                        
                        await HttpContext.Authentication.SignInAsync(ApplicationConst.PilotMiddlewareInstanceName, principal);
                        
                        return RedirectToAction("Index", "Home");
                    }
                    catch (JsonReaderException)
                    {
                        var databaseNotFoundMessage = string.Format("Database [{0}] not found", model.DatabaseName);
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

        public async Task<IActionResult> LogOff()
        {
            await HttpContext.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
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
