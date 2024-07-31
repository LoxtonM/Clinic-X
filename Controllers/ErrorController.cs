﻿using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ClinicX.Controllers
{
    public class ErrorController : Controller
    {       
        [HttpGet]
        public IActionResult ErrorHome(string error, string? formName="")
        {
            //using (StreamWriter sw = new StreamWriter(Path.Combine($"wwwroot/ErrorLogs/Log-{DateTime.Now}.txt"), true))
            //{
            //    sw.WriteLine($"{DateTime.Now}: {error} - logged in user: {User.Identity.Name}");
            //}

            using (StreamWriter sw = System.IO.File.AppendText($"wwwroot/ErrorLogs/Log.txt"))
            {
                sw.WriteLine($"{DateTime.Now}: {error} - logged in user: {User.Identity.Name} - form: {formName}");
            }

            return View("ErrorHome", error);
        }
    }
}
