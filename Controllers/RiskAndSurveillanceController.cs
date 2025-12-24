//using APIControllers.Models;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
//using ClinicX.Data;
using ClinicX.Meta;
//using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RiskAndSurveillanceController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        private readonly RiskSurveillanceVM _rsvm;
        private readonly IConfiguration _config;        
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly ITriageData _triageData;
        private readonly IRiskData _riskData;
        private readonly ISurveillanceData _survData;
        private readonly IRiskCodesData _riskCodesData;
        private readonly ISurveillanceCodesData _survCodesData;
        private readonly ITestEligibilityData _testEligibilityData;
        private readonly IMiscData _misc;
        private readonly IGeneChangeData _geneChange;
        private readonly IGeneCodeData _geneCode;
        private readonly IRelativeData _relData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public RiskAndSurveillanceController(IConfiguration config, IPatientData patientData, IStaffUserData staffUserData, ITriageData triageData, IRiskData riskData, 
            ISurveillanceData surveillanceData, IRiskCodesData riskCodesData, ISurveillanceCodesData surveillanceCodesData, ITestEligibilityData testEligibilityData,
            IMiscData miscData, IGeneChangeData geneChangeData, IGeneCodeData geneCodeData, IRelativeData relativeData, ICRUD crud,IAuditService auditService)
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            _config = config;
            _rsvm = new RiskSurveillanceVM();
            //_patientData = new PatientData(_clinContext);
            _patientData = patientData;
            //_staffUser = new StaffUserData(_clinContext);
            _staffUser = staffUserData;
            //_triageData = new TriageData(_clinContext);
            _triageData = triageData;
            //_riskData = new RiskData(_clinContext);
            _riskData = riskData;
            //_survData = new SurveillanceData(_clinContext);
            _survData = surveillanceData;
            //_riskCodesData = new RiskCodesData(_cXContext);
            _riskCodesData = riskCodesData;
            //_survCodesData = new SurveillanceCodesData(_cXContext);
            _survCodesData = surveillanceCodesData;
            //_testEligibilityData = new TestEligibilityData(_clinContext);
            _testEligibilityData = testEligibilityData;
            //_misc = new MiscData(_config);
            _misc = miscData;
            //_geneChange = new GeneChangeData(_cXContext);
            _geneChange = geneChangeData;
            //_geneCode = new GeneCodeData(_cXContext);
            _geneCode = geneCodeData;
            //_relData = new RelativeData(_clinContext);
            _relData = relativeData;
            //_crud = new CRUD(_config);
            _crud = crud;
            //_audit = new AuditService(_config);
            _audit = auditService;
        }

        [Authorize]
        public IActionResult Index(int mpi)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk List", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _rsvm.patient = _patientData.GetPatientDetails(mpi);
                _rsvm.riskList = _riskData.GetRiskListForPatient(mpi);                

                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [Authorize]
        public IActionResult RiskDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rsvm.geneChange = _geneChange.GetGeneChangeList();
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.surveillanceList = _survData.GetSurveillanceListByRiskID(_rsvm.riskDetails.RiskID);
                _rsvm.eligibilityList = _testEligibilityData.GetTestingEligibilityList(_rsvm.riskDetails.MPI);
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult SurvDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rsvm.geneChange= _geneChange.GetGeneChangeList();
                _rsvm.surveillanceDetails = _survData.GetSurvDetails(id);
                _rsvm.riskDetails = _riskData.GetRiskDetails(_rsvm.surveillanceDetails.RiskID);
                int mpi = _rsvm.surveillanceDetails.MPI;

                _rsvm.patient = _patientData.GetPatientDetails(mpi);
                
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpPost]
        public IActionResult SurvDetails(int survID, int geneChange)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + survID.ToString(), _ip.GetIPAddress());                

                int riskID = _survData.GetSurvDetails(survID).RiskID;                

                int success = _crud.CallStoredProcedure("Surveillance", "Add Gene Change", survID, geneChange, 0, "", "", "", "",
                    User.Identity.Name, null, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addRisk(QSL)" }); }
                               
                
                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = riskID });               
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "SurvDetails" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult AddNewRisk(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Risk", "ICPID=" + id.ToString(), _ip.GetIPAddress());

                _rsvm.icpCancer = _triageData.GetCancerICPDetailsByICPID(id);
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.patient = _patientData.GetPatientDetails(_rsvm.icpCancer.MPI);
                _rsvm.refID = _triageData.GetICPDetails(id).REFID;
                _rsvm.riskCodes = _riskCodesData.GetRiskCodesList();
                _rsvm.survSiteCodes = _survCodesData.GetSurvSiteCodesList();
                _rsvm.staffMembersList = _staffUser.GetClinicalStaffList();
                _rsvm.staffCode = staffCode;
                _rsvm.calculationTools = _riskCodesData.GetCalculationToolsList();
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addRisk" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewRisk(int refID, string riskCode, string siteCode, string clinCode, 
            DateTime riskDate, float lifetimePercent, string comments, float f2529, float f3040, float f4050, 
            float f5060, bool isUseLetter, string tool)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Risk", "Create", refID, 0, 0, riskCode, siteCode, clinCode, comments,
                    User.Identity.Name, riskDate, null, isUseLetter, false, 0, 0, 0, tool, "", "", lifetimePercent,
                    f2529, f3040, f4050, f5060);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addRisk(QSL)" }); }

                int riskID = _misc.GetRiskID(refID); //this function simply gets the latest RiskID related to the RefID
                int icpID = _riskData.GetRiskDetails(riskID).ICPID;
                int icpCancerID = _triageData.GetCancerICPDetailsByICPID(icpID).ICP_Cancer_ID; //this is all just to get the ICPCancerID!

                return RedirectToAction("CancerReview", "Triage", new { id = icpCancerID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addRisk" });
            }
        }        

        [HttpGet]
        [Authorize]
        public IActionResult AddNewSurveillance(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Surveillance", "RiskID=" + id.ToString(), _ip.GetIPAddress());

                _rsvm.riskID = id;
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.patient = _patientData.GetPatientDetails(_rsvm.riskDetails.MPI);
                _rsvm.survSiteCodes = _survCodesData.GetSurvSiteCodesList();
                _rsvm.survTypeCodes = _survCodesData.GetSurvTypeCodesList();
                _rsvm.survFreqCodes = _survCodesData.GetSurvFreqCodesList();
                _rsvm.discontinuedReasonCodes = _survCodesData.GetDiscReasonCodesList();
                _rsvm.staffMembersList = _staffUser.GetClinicalStaffList();

                AgeCalculator ageCalc = new AgeCalculator(); //to display patient's current age (requested feature)

                int ddYear = ageCalc.DateDifferenceYear(_rsvm.patient.DOB.GetValueOrDefault(), DateTime.Now);
                int ddMonth = ageCalc.DateDifferenceMonth(_rsvm.patient.DOB.GetValueOrDefault(), DateTime.Now);
                
                if(ddMonth <= 0)
                {
                    ddYear -= 1;
                    ddMonth += 12; 
                }

                _rsvm.patientAge = $"{ddYear} years {ddMonth} months";

                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addSurv" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewSurveillance(int riskID, string siteCode, string typeCode, string clinCode, 
            DateTime recDate, int startAge, int endAge, string frequency, bool isUseLetter, bool isYN, string? discReason)
        {
            try
            {
                if(discReason == null) { discReason = ""; } //because SQL doesn't like nulls
                
                int success = _crud.CallStoredProcedure("Surveillance", "Create", riskID, startAge, endAge, siteCode, typeCode, clinCode, "",
                    User.Identity.Name, recDate, null, isUseLetter, isYN, 0, 0, 0, frequency, discReason);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addSurv(SQL)" }); }

                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = riskID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addSurv" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditRisk(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rsvm.geneChange = _geneChange.GetGeneChangeList();
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.surveillanceList = _survData.GetSurveillanceListByRiskID(_rsvm.riskDetails.RiskID);
                _rsvm.eligibilityList = _testEligibilityData.GetTestingEligibilityList(_rsvm.riskDetails.MPI);
                _rsvm.riskCodes = _riskCodesData.GetRiskCodesList();
                _rsvm.survSiteCodes = _survCodesData.GetSurvSiteCodesList();
                _rsvm.calculationTools = _riskCodesData.GetCalculationToolsList();
                _rsvm.staffMembersList = _staffUser.GetClinicalStaffList();
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpPost]
        public IActionResult EditRisk(int id, string riskCode, string siteCode, string clinCode,
            DateTime riskDate, float lifetimePercent, string comments, float f2529, float f3040, float f4050,
            float f5060, bool isUseLetter, string tool)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());

                _crud.CallStoredProcedure("Risk", "Edit", id, 0, 0, riskCode, siteCode, clinCode, comments, User.Identity.Name, null, null, isUseLetter, false,
                    0, 0, 0, "", tool, "", f2529, f3040, f4050, f5060, lifetimePercent);

                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditSurveillance(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rsvm.geneChange = _geneChange.GetGeneChangeList();
                _rsvm.surveillanceDetails = _survData.GetSurvDetails(id);
                _rsvm.riskDetails = _riskData.GetRiskDetails(_rsvm.surveillanceDetails.RiskID);
                _rsvm.survFreqCodes = _survCodesData.GetSurvFreqCodesList();
                _rsvm.discontinuedReasonCodes = _survCodesData.GetDiscReasonCodesList();
                int mpi = _rsvm.surveillanceDetails.MPI;

                _rsvm.patient = _patientData.GetPatientDetails(mpi);

                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpPost]
        public IActionResult EditSurveillance(int id, string geneChange, DateTime discDate, int startAge, int endAge, string frequency, bool isDisc, string? discReason)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString(), _ip.GetIPAddress());

                if(discDate == null || discDate == DateTime.Parse("0001-01-01")) { discDate = DateTime.Parse("1900-01-01"); } //because Javascript has it as "0001-01-01" but SQL needs it to be "1900-01-01"

                _crud.CallStoredProcedure("Surveillance", "Edit", id, startAge, endAge, geneChange, "", frequency, discReason, User.Identity.Name, discDate, null, isDisc, false);

                return RedirectToAction("SurvDetails", "RiskAndSurveillance", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }



        [HttpGet]
        [Authorize]
        public IActionResult AddNewTestingEligibility(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Risk", "ICPID=" + id.ToString(), _ip.GetIPAddress());

                _rsvm.icpCancer = _triageData.GetCancerICPDetails(id);
                _rsvm.patient = _patientData.GetPatientDetails(_rsvm.icpCancer.MPI);
                _rsvm.eligibilityList = _testEligibilityData.GetTestingEligibilityList(_rsvm.patient.MPI);
                _rsvm.refID = _rsvm.icpCancer.RefID;                
                _rsvm.geneCode = _geneCode.GetGeneCodeList();
                _rsvm.staffCode = staffCode;
                _rsvm.calculationTools = _riskCodesData.GetCalculationToolsList();
                _rsvm.relatives = _relData.GetRelativesList(_rsvm.patient.MPI);
                
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addRisk" });
            }
        }

        [HttpPost]
        public IActionResult AddNewTestingEligibility(int refID, int gene, string tool, string score, string offerTest, int? relative=0)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Testing Eligibility", "", _ip.GetIPAddress());

                ICP icp = _triageData.GetICPDetailsByRefID(refID);
                _rsvm.icpCancer = _triageData.GetCancerICPDetailsByICPID(icp.ICPID);
                                
                bool isRelative = false; //set a bool based in an int
                if(relative != 0) { isRelative = true; }

                if (score == null) { score = ""; }

                int success = _crud.CallStoredProcedure("TestEligibility", "Create", refID, relative.GetValueOrDefault(), gene, tool, score, offerTest, "",
                User.Identity.Name, null, null, isRelative);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addSurv(SQL)" }); }

                return RedirectToAction("CancerReview", "Triage", new { id = _rsvm.icpCancer.ICP_Cancer_ID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addTestEli" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditTestingEligibilityDetails(int id)
        {
            _rsvm.eligibilityDetails = _testEligibilityData.GetTestingEligibilityDetails(id);
            _rsvm.patient = _patientData.GetPatientDetails(_rsvm.eligibilityDetails.MPI);            
            _rsvm.relatives = _relData.GetRelativesList(_rsvm.patient.MPI);
            ICP icp = _triageData.GetICPDetailsByRefID(_rsvm.eligibilityDetails.RefID);
            _rsvm.icpCancer = _triageData.GetCancerICPDetailsByICPID(icp.ICPID);
            _rsvm.geneCode = _geneCode.GetGeneCodeList();
            _rsvm.calculationTools = _riskCodesData.GetCalculationToolsList();

            return View(_rsvm);
        }

        [HttpPost]
        public IActionResult EditTestingEligibilityDetails(int id, int gene, string tool, string score, string offerTest)
        {
            _rsvm.eligibilityDetails = _testEligibilityData.GetTestingEligibilityDetails(id);
            ICP icp = _triageData.GetICPDetailsByRefID(_rsvm.eligibilityDetails.RefID);
            _rsvm.icpCancer = _triageData.GetCancerICPDetailsByICPID(icp.ICPID);

            int success = _crud.CallStoredProcedure("TestEligibility", "Edit", id, gene, 0, tool, score, offerTest, "",
                User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addSurv(SQL)" }); }

            //return RedirectToAction("Index", "WIP");
            return RedirectToAction("CancerReview", "Triage", new { id = _rsvm.icpCancer.ICP_Cancer_ID });
        }

        [Authorize]
        public IActionResult SetUsingLetter(int id, bool isUsingLetter) //to provide a quick "don't use this one!" mechanic
        {
            _rsvm.riskDetails = _riskData.GetRiskDetails(id);
            int icpID = _rsvm.riskDetails.ICP_Cancer_ID;

            _crud.CallStoredProcedure("Risk", "Set Use Letter", id, 0, 0, "", "", "", "", User.Identity.Name, null, null, isUsingLetter, false);

            return RedirectToAction("CancerReview", "Triage", new { id = icpID });
        }
    }
}
