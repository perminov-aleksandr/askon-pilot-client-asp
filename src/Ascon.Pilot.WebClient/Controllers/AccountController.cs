using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var serializedData = SerializeOpenDatabaseRequestData(model);

                var baseAddress = new Uri(ApplicationConst.PilotServerUrl);
                using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    var result = await client.PostAsync(ApplicationConst.PilotServerConnectUrl, new StringContent(string.Empty));
                    if (!result.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Не удается подключиться к серверу");
                        return View(model);
                    }

                    var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
                    result = await client.PostAsync(ApplicationConst.PilotServerCallApiUrl, content);
                    var resultContent = await result.Content.ReadAsStringAsync();

                    try
                    {
                        var deserializedResult = JsonConvert.DeserializeObject(resultContent);
                        return Json("Вы успешно авторизованы в системе");
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

        private static string SerializeOpenDatabaseRequestData(LogInViewModel model)
        {
            var openDatabaseRequest = new OpenDatabaseRequest
            {
                licenseType = 100,
                api = ApplicationConst.PilotServerAppName,
                method = ApplicationConst.OpenDatabaseMethod,
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
