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
        private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM cvm;
        private readonly VMData _vm;

        public HomeController(ClinicalContext context)
        {
            _clinContext = context;
            cvm = new CaseloadVM();
            _vm = new VMData(_clinContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = _vm.GetCurrentStaffUser(User.Identity.Name);
                
                cvm.caseLoad = _vm.GetCaseloadList(user.STAFF_CODE).OrderBy(c => c.BookedDate).ToList();
                cvm.countClinics = cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                cvm.countTriages = cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                cvm.countCancerICPs = cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                cvm.countTests = cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                cvm.countReviews = cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                cvm.countLetters = cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                cvm.name = cvm.caseLoad.FirstOrDefault().Clinician;
                

                return View(cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
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