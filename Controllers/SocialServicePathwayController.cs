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
            _sspvm.socialServicePathway = await _socialServicePathwayData.GetSocialServicePathwayById(id);

            return View(_sspvm);
        }
    }
}
