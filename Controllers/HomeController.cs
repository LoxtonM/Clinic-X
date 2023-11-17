using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ClinicX.Data;
using ClinicX.Models;

namespace ClinicX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClinicalContext _context;
        //here is a random comment for no reason
        public HomeController(ILogger<HomeController> logger, ClinicalContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);
            var caseload = from cl in _context.Caseload
                    where cl.StaffCode == user.STAFF_CODE
                    orderby cl.BookedDate
                    select cl;

            //return View(user);
            return View(await caseload.ToListAsync());
            //return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("UserLogin","Login");
        }
    }
}