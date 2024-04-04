using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RiskAndSurveillanceController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly RiskSurveillanceVM _rsvm;
        private readonly VMData _vm;
        private readonly MiscData _misc;
        private readonly CRUD _crud;

        public RiskAndSurveillanceController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rsvm = new RiskSurveillanceVM();
            _vm = new VMData(_clinContext);
            _misc = new MiscData(_config);
            _crud = new CRUD(_config);
        }

        public IActionResult RiskDetails(int id)
        {
            try
            {
                _rsvm.riskDetails = _vm.GetRiskDetails(id);
                _rsvm.surveillanceList = _vm.GetSurveillanceListByRiskID(_rsvm.riskDetails.RiskID);
                _rsvm.eligibilityList = _vm.GetTestingEligibilityList(_rsvm.riskDetails.MPI);
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewRisk(int id)
        {
            try
            {
                _rsvm.icpCancer = _vm.GetCancerICPDetailsByICPID(id);
                _rsvm.patient = _vm.GetPatientDetails(_rsvm.icpCancer.MPI);
                _rsvm.refID = _vm.GetICPDetails(id).REFID;
                _rsvm.riskCodes = _vm.GetRiskCodesList();
                _rsvm.survSiteCodes = _vm.GetSurvSiteCodesList();
                _rsvm.staffMembersList = _vm.GetClinicalStaffList();
                _rsvm.calculationTools = _vm.GetCalculationToolsList();
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewRisk(int refID, string riskCode, string siteCode, string clinCode, 
            DateTime riskDate, float lifetimePercent, string comments, float f2529, float f3040, float f4050, 
            float f5060, bool isUseLetter, string tool)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Risk", "Create", refID, 0, 0, riskCode, siteCode, clinCode, comments,
                    User.Identity.Name, riskDate, null, isUseLetter, false, 0, 0, 0, tool, "", "", lifetimePercent,
                    f2529, f3040, f4050, f5060);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                int riskID = _misc.GetRiskID(refID);
                int icpID = _vm.GetRiskDetails(riskID).ICPID;
                int icpCancerID = _vm.GetCancerICPDetailsByICPID(icpID).ICP_Cancer_ID;
                return RedirectToAction("CancerReview", "Triage", new { id = icpCancerID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewSurveillance(int id)
        {
            try
            {
                _rsvm.riskID = id;
                _rsvm.riskDetails = _vm.GetRiskDetails(id);
                _rsvm.patient = _vm.GetPatientDetails(_rsvm.riskDetails.MPI);
                _rsvm.survSiteCodes = _vm.GetSurvSiteCodesList();
                _rsvm.survTypeCodes = _vm.GetSurvTypeCodesList();
                _rsvm.survFreqCodes = _vm.GetSurvFreqCodesList();
                _rsvm.discontinuedReasonCodes = _vm.GetDiscReasonCodesList();
                _rsvm.staffMembersList = _vm.GetClinicalStaffList();
                
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewSurveillance(int riskID, string siteCode, string typeCode, string clinCode, 
            DateTime recDate, int startAge, int endAge, string frequency, bool isUseLetter, bool isYN, string? discReason)
        {
            try
            {
                if(discReason == null)
                {
                    discReason = ""; //because it can't simply evaluate the optional parameter as an empty string for some reason!!!
                }
                
                int success = _crud.CallStoredProcedure("Surveillance", "Create", riskID, startAge, endAge, siteCode, typeCode, clinCode, "",
                    User.Identity.Name, recDate, null, isUseLetter, isYN, 0, 0, 0, frequency, discReason);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = riskID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
