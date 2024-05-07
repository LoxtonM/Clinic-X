using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class HomeController : Controller
    {        
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly CaseloadVM _cvm;
        private readonly CaseloadData _caseload;
        private readonly StaffUserData _staffUser;

        public HomeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();
            _caseload = new CaseloadData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);

        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {                
                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                _cvm.caseLoad = _caseload.GetCaseloadList(user.STAFF_CODE).OrderBy(c => c.BookedDate).ToList();
                _cvm.countClinics = _cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                _cvm.countTriages = _cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                _cvm.countCancerICPs = _cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                _cvm.countTests = _cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                _cvm.countReviews = _cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                _cvm.countLetters = _cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                if (_cvm.caseLoad.Count > 0)
                {
                    _cvm.name = _cvm.caseLoad.FirstOrDefault().Clinician;
                }
                else
                {
                    _cvm.name = "";
                }

                _cvm.isLive = bool.Parse(_config.GetValue("IsLive", ""));

                return View(_cvm);
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