using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class ErrorController : Controller
    {       
        [HttpGet]
        public IActionResult ErrorHome(string error)
        {            
            return View("ErrorHome", error);
        }
    }
}
