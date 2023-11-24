using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class ErrorController : Controller
    {
        public string strError { get; set; }
        public IActionResult ErrorHome(string sError)
        {
            strError = sError;
            return View();
        }
    }
}
