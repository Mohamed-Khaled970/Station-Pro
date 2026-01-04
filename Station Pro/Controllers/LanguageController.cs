using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Station_Pro.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILogger<LanguageController> _logger;

        public LanguageController(ILogger<LanguageController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
           CookieRequestCultureProvider.DefaultCookieName,
           CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
           new CookieOptions
           {
               Expires = DateTimeOffset.UtcNow.AddYears(1),
               IsEssential = true,
               Path = "/"
           }
       );

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}