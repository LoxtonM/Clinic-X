using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Meta;

namespace ClinicX.Controllers
{
    public class HomeController : Controller
    {        
        private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM _cvm;
        private readonly IConfiguration _config;        
        private readonly ICaseloadData _caseload;
        private readonly IStaffUserData _staffUser;
        private readonly IVersionData _version;
        private readonly INotificationData _notificationData;
        private readonly IAuditService _audit;

        public HomeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();
            _caseload = new CaseloadData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _version = new VersionData();
            _notificationData = new NotificationData(_clinContext);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _cvm.notificationMessage = _notificationData.GetMessage();
                    var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                    _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Home");
                    _cvm.caseLoad = _caseload.GetCaseloadList(user.STAFF_CODE).OrderBy(c => c.BookedDate).ToList();
                    _cvm.countClinics = _cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                    _cvm.countTriages = _cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                    _cvm.countCancerICPs = _cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                    _cvm.countTests = _cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                    _cvm.countReviews = _cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                    _cvm.countLetters = _cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                    _cvm.dllVersion = _version.GetDLLVersion();
                    _cvm.appVersion = _config.GetValue("AppVersion", "");
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
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Home" });
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