using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class WIPController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> NotFound()
        {
            return View();
        }
    }
}
