using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class ErrorController : Controller
    {
        //public string strError; //{ get; set; }
        
        [HttpGet]
        public IActionResult ErrorHome(string sError)
        {            
            return View("ErrorHome", sError);
        }
    }
}
