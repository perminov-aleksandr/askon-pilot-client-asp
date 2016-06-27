using Ascon.Pilot.WebClient.Extensions;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    /// <summary>
    /// Контроллер модели Home
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Представление Index
        /// </summary>
        /// <returns>представление Index</returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Files");
        }
        /// <summary>
        /// Представление Types
        /// </summary>
        /// <returns>представление Types</returns>
        public IActionResult Types()
        {
            return View(HttpContext.Session.GetMetatypes().Values);
        }

        /// <summary>
        /// Установление типа иконок файлов для папки id
        /// </summary>
        /// <param name="id">уникальный идетификатор папки</param>
        /// <returns>представления иконок файлов для разных типов файлов</returns>
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
        /// <summary>
        /// Сообщения об ошибках
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке</param>
        /// <returns>Представление сообщения об ошибке</returns>
        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
    }
}
