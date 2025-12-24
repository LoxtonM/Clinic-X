//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class HomeController : Controller
    {        
        //private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM _cvm;
        private readonly IConfiguration _config;        
        private readonly ICaseloadDataAsync _caseload;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IVersionData _version;
        private readonly INotificationDataAsync _notificationData;
        private readonly IAuditService _audit;        

        public HomeController(IConfiguration config, ICaseloadDataAsync caseload, IStaffUserDataAsync staffUser, IVersionData version, INotificationDataAsync notificationData,
        IAuditService audit)
        {
            //_clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();
            _caseload = caseload;
            _staffUser = staffUser;
            _version = version;
            _notificationData = notificationData;
            _audit = audit;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                _cvm.notificationMessage = await _notificationData.GetMessage("ClinicXOutage"); //messaging system for outages
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                //when switching from Dev to Live, it's possible to be "authenticated" with a user that doesn't exist
                if (user == null) { return RedirectToAction("ErrorHome", "Error", new { error = "User not found, please sign out and sign in again", formName = "Home" }); }
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Home", "", _ip.GetIPAddress());
                _cvm.caseLoad = await _caseload.GetCaseloadList(user.STAFF_CODE);
                    //count each instance of caseload type for telemetry

                _cvm.countClinics = _cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                _cvm.countTriages = _cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                _cvm.countCancerICPs = _cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                _cvm.countTests = _cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                _cvm.countReviews = _cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                _cvm.countLetters = _cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                
                _cvm.dllVersion = _version.GetDLLVersion(); //display version of data library                
                _cvm.appVersion = _config.GetValue<string>("AppVersion");                
                _cvm.isLive = _config.GetValue<bool>("IsLive");
                _cvm.name = user.NAME;

                return View(_cvm);
                
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