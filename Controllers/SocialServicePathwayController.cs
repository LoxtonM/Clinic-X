using ClinicalXPDataConnections.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class SocialServicePathwayController : Controller
    {
        private readonly SocialServicePathwayVM _sspvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly ISocialServicePathwayDataAsync _socialServicePathwayData;
        private readonly IAuditService _audit;

        public SocialServicePathwayController(IConfiguration config, IStaffUserDataAsync staffUserData, IAuditService auditService, ISocialServicePathwayDataAsync socialServicePathwayData)
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            _config = config;
            _staffUser = staffUserData;
            _socialServicePathwayData = socialServicePathwayData;
            _audit = auditService;
            _sspvm = new SocialServicePathwayVM();
        }
        public async Task<IActionResult> Index()
        {
            _sspvm.socialServicePathwayList = await _socialServicePathwayData.GetAllSocialServicePathways();

            return View(_sspvm);
        }

        public async Task<IActionResult> SSPDetails(int id)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - SSP", "id=" + id.ToString(), _ip.GetIPAddress());

            _sspvm.socialServicePathway = await _socialServicePathwayData.GetSocialServicePathwayById(id);

            return View(_sspvm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int sspID)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - SSP Edit", "id=" + sspID.ToString(), _ip.GetIPAddress());

            _sspvm.socialServicePathway = await _socialServicePathwayData.GetSocialServicePathwayById(sspID);
            _sspvm.socialServicePathwayStatusList = await _socialServicePathwayData.GetSocialServicePathwayStatuses();
            _sspvm.socialServicePathwayOutcomeList = await _socialServicePathwayData.GetSocialServicePathwayOutcomes();
            _sspvm.socialWorkerList = await _socialServicePathwayData.GetSocialWorkers();

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int sspID, string something)
        {


            return RedirectToAction("SSPDetails", new { id = sspID });
        }
    }
}
