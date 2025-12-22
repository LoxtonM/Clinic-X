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
        private readonly ICaseloadData _caseload;
        private readonly IStaffUserData _staffUser;
        private readonly IVersionData _version;
        private readonly INotificationData _notificationData;
        private readonly IAuditService _audit;        

        public HomeController(IConfiguration config, ICaseloadData caseload, IStaffUserData staffUser, IVersionData version, INotificationData notificationData,
        IAuditService audit)
        {
            //_clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();
            //_caseload = new CaseloadData(_clinContext);
            _caseload = caseload;
            //_staffUser = new StaffUserData(_clinContext);
            _staffUser = staffUser;
            //_version = new VersionData();
            _version = version;
            //_notificationData = new NotificationData(_clinContext);
            _notificationData = notificationData;
            //_audit = new AuditService(_config);
            _audit = audit;

        }

        [Authorize]
        public IActionResult Index()
        {
            try
            {
                _cvm.notificationMessage = _notificationData.GetMessage("ClinicXOutage"); //messaging system for outages
                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                //when switching from Dev to Live, it's possible to be "authenticated" with a user that doesn't exist
                if (user == null) { return RedirectToAction("ErrorHome", "Error", new { error = "User not found, please sign out and sign in again", formName = "Home" }); }
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Home", "", _ip.GetIPAddress());
                _cvm.caseLoad = _caseload.GetCaseloadList(user.STAFF_CODE);
                    //count each instance of caseload type for telemetry
                _cvm.countClinics = _cvm.caseLoad.Where(c => c.Type.Contains("App")).Count();
                _cvm.countTriages = _cvm.caseLoad.Where(c => c.Type.Contains("Triage")).Count();
                _cvm.countCancerICPs = _cvm.caseLoad.Where(c => c.Type.Contains("Cancer")).Count();
                _cvm.countTests = _cvm.caseLoad.Where(c => c.Type.Contains("Test")).Count();
                _cvm.countReviews = _cvm.caseLoad.Where(c => c.Type.Contains("Review")).Count();
                _cvm.countLetters = _cvm.caseLoad.Where(c => c.Type.Contains("Letter")).Count();
                _cvm.dllVersion = _version.GetDLLVersion(); //display version of data library
                _cvm.appVersion = _config.GetValue("AppVersion", ""); //display version from json file
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