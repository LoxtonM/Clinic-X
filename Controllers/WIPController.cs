using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class WIPController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}
