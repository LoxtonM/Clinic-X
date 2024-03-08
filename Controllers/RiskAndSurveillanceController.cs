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
        private readonly RiskSurveillanceVM rsvm;
        private readonly VMData vm;
        private readonly MiscData misc;
        private readonly CRUD crud;

        public RiskAndSurveillanceController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            rsvm = new RiskSurveillanceVM();
            vm = new VMData(_clinContext);
            misc = new MiscData(_config);
            crud = new CRUD(_config);
        }

        public IActionResult RiskDetails(int id)
        {
            try
            {
                rsvm.riskDetails = vm.GetRiskDetails(id);
                rsvm.surveillanceList = vm.GetSurveillanceListByRiskID(rsvm.riskDetails.RiskID);
                rsvm.eligibilityList = vm.GetTestingEligibilityList(rsvm.riskDetails.MPI);
                return View(rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewRisk(int id)
        {
            try
            {
                rsvm.icpCancer = vm.GetCancerICPDetailsByICPID(id);
                rsvm.patient = vm.GetPatientDetails(rsvm.icpCancer.MPI);
                rsvm.iRefID = vm.GetICPDetails(id).REFID;
                rsvm.riskCodes = vm.GetRiskCodesList();
                rsvm.survSiteCodes = vm.GetSurvSiteCodesList();
                rsvm.staffMembersList = vm.GetClinicalStaffList();
                rsvm.calculationTools = vm.GetCalculationToolsList();
                return View(rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewRisk(int iRefID, string sRiskCode, string sSiteCode, string sClinCode, 
            DateTime dRiskDate, float fLifetimePercent, string sComments, float f2529, float f3040, float f4050, 
            float f5060, bool isUseLetter, string sTool)
        {
            try
            {
                crud.CallStoredProcedure("Risk", "Create", iRefID, 0, 0, sRiskCode, sSiteCode, sClinCode, sComments,
                    User.Identity.Name, dRiskDate, null, isUseLetter, false, 0, 0, 0, sTool, "", "", fLifetimePercent,
                    f2529, f3040, f4050, f5060);
                int iRiskID = misc.GetRiskID(iRefID);
                int icpID = vm.GetRiskDetails(iRiskID).ICPID;
                int iID = vm.GetCancerICPDetailsByICPID(icpID).ICP_Cancer_ID;
                return RedirectToAction("CancerReview", "Triage", new { id = iID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewSurveillance(int id)
        {
            try
            {
                rsvm.iRiskID = id;
                rsvm.riskDetails = vm.GetRiskDetails(id);
                rsvm.patient = vm.GetPatientDetails(rsvm.riskDetails.MPI);
                rsvm.survSiteCodes = vm.GetSurvSiteCodesList();
                rsvm.survTypeCodes = vm.GetSurvTypeCodesList();
                rsvm.survFreqCodes = vm.GetSurvFreqCodesList();
                rsvm.discontinuedReasonCodes = vm.GetDiscReasonCodesList();
                rsvm.staffMembersList = vm.GetClinicalStaffList();
                
                return View(rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewSurveillance(int iRiskID, string sSiteCode, string sTypeCode, string sClinCode, 
            DateTime dRecDate, int iStartAge, int iEndAge, string sFrequency, bool isUseLetter, bool isYN, string? sDiscReason)
        {
            try
            {
                if(sDiscReason == null)
                {
                    sDiscReason = ""; //because it can't simply evaluate the optional parameter as an empty string for some reason!!!
                }
                crud.CallStoredProcedure("Surveillance", "Create", iRiskID, iStartAge, iEndAge, sSiteCode, sTypeCode, sClinCode, "",
                    User.Identity.Name, dRecDate, null, isUseLetter, isYN, 0, 0, 0, sFrequency, sDiscReason);
                
                
                
                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = iRiskID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
