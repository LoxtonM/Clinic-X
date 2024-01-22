using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class HomeController : Controller
    {        
        private readonly ClinicalContext _context;
        private readonly CaseloadVM cvm;
        private readonly VMData vm;

        public HomeController(ClinicalContext context)
        {
            _context = context;
            cvm = new CaseloadVM();
            vm = new VMData(_context);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);
                //var caseload = from cl in _context.Caseload
                //              where cl.StaffCode == user.STAFF_CODE
                //              orderby cl.BookedDate
                //              select cl;
                cvm.caseLoad = vm.GetCaseload(user.STAFF_CODE);
                cvm.countClinics = cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                cvm.countTriages = cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                cvm.countCancerICPs = cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                cvm.countTests = cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                cvm.countReviews = cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                cvm.countLetters = cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                cvm.name = cvm.caseLoad.FirstOrDefault().Clinician;
                //return View(await caseload.ToListAsync());                
                return View(cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
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