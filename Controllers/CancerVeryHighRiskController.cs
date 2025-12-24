//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
//using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class CancerVeryHighRiskController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        //private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IReferralDataAsync _referralData;
        private readonly ITriageDataAsync _triageData;
        private readonly IDiaryDataAsync _diaryData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IPatientDataAsync _patientData;
        private readonly IUntestedVHRGroupDataAsync _untestedVHRGroupData;
        private readonly CancerVeryHighRiskVM _cvm;
        private readonly IScreeningServiceDataAsync _ssData;
        private readonly VHRController _vhrc;

        public CancerVeryHighRiskController(IConfiguration config, IStaffUserDataAsync staffUserData, IReferralDataAsync referralData, ITriageDataAsync triageData, IDiaryDataAsync diaryData, ICRUD crud, 
            IAuditService auditService, IPatientDataAsync patientData, IUntestedVHRGroupDataAsync untestedVHRGroupData, IScreeningServiceDataAsync ssData, VHRController vhrc)
        {
            _config = config;
            //_clinContext = clinContext;
            //_cXContext = cxContext;
            //_docContext = docContext;
            _staffUser = staffUserData;
            _referralData = referralData;
            _triageData = triageData;
            _diaryData = diaryData;
            _crud = crud;
            _audit = auditService;
            _patientData = patientData;
            _untestedVHRGroupData = untestedVHRGroupData;
            _cvm = new CancerVeryHighRiskVM();
            _ssData = ssData;
            _vhrc = vhrc;            
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> VHRPro(int id, string? message, bool? success)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Cancer Post Clinic Letter", "ID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.icpCancer = await _triageData.GetCancerICPDetails(id);
                
                var icpDetails = await _triageData.GetICPDetails(_cvm.icpCancer.ICPID);
                
                _cvm.screeningCoordinators = await _ssData.GetScreeningServiceList();

                int mpi = icpDetails.MPI;
                var pat = await _patientData.GetPatientDetails(mpi);

                ScreeningService ss = await _ssData.GetScreeningServiceDetails(pat.GP_Facility_Code);
                _cvm.defaultScreeningCo = ss.ScreeningOfficeCode;               

                int refID = icpDetails.REFID;
                _cvm.referral = await _referralData.GetReferralDetails(refID);

                _cvm.untestedVHRGroup = await _untestedVHRGroupData.GetUntestedVHRGroupDataByRefID(refID);

                _cvm.success = success.GetValueOrDefault();
                _cvm.message = message;

                return View(_cvm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-vhrpro" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VHRPro(int id, string? freeText, string? screeningService, bool? isPreview = false)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Cancer Post Clinic Letter", "ID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.icpCancer = await _triageData.GetCancerICPDetails(id);
                var icpDetails = await _triageData.GetICPDetails(_cvm.icpCancer.ICPID);
                //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
                int mpi = icpDetails.MPI;
                int refID = icpDetails.REFID;
                var refer = await _referralData.GetReferralDetails(refID);
                string referCode = refer.ReferrerCode;

                if (isPreview.GetValueOrDefault())
                {                    
                    _vhrc.DoVHRPro(213, mpi, id, User.Identity.Name, referCode, screeningService, freeText, 0, true);

                    return File($"~/StandardLetterPreviews/VHRPropreview-{User.Identity.Name}.pdf", "Application/PDF");
                }
                else
                {
                    string docCode = "VHRPro";
                    string diaryText = "";

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", diaryText, User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    int diaryID = diary.DiaryID;
                    _vhrc.DoVHRPro(213, mpi, id, User.Identity.Name, referCode, screeningService, freeText, diaryID, false);

                    docCode = "VHRProC";
                    diaryText = "";
                    successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", diaryText, User.Identity.Name, null, null, false, false);
                    var diary2 = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    diaryID = diary2.DiaryID;

                    _vhrc.DoVHRProCoverLetter(214, mpi, id, User.Identity.Name, referCode, screeningService, freeText, diaryID);                    

                    bool iSuccess = true;
                    string sMessage = "VHRPro created for filing in EDMS";

                    return RedirectToAction("CancerReview", new { id = id, success = iSuccess, message = sMessage });
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "VHRpro" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditUntestedVHROptions(int id, int? thresholdAge, bool below30, bool below50, bool other, bool notID)
        {
            try
            {                
                int iOther = other ? 1 : 0;
                int iNotID = notID ? 1 : 0;                

                int success = _crud.CallStoredProcedure("UntestedVHRGroup", "Edit", id, thresholdAge.GetValueOrDefault(), 0, "", "", "", "", User.Identity.Name, null, null, below30, below50, iOther, iNotID);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "VHRPro update untestedgroup" }); }

                bool iSuccess = true;
                string sMessage = "Saved!";

                UntestedVHRGroup utg = await _untestedVHRGroupData.GetUntestedVHRGroupData(id);
                ICP icp = await _triageData.GetICPDetailsByRefID(utg.RefID.GetValueOrDefault());
                ICPCancer icpc = await _triageData.GetCancerICPDetailsByICPID(icp.ICPID);

                return RedirectToAction("VHRPro", new { id = icpc.ICP_Cancer_ID, success = iSuccess, message = sMessage });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "VHRpro-Untested" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> NewUntestedVHROptions(int refid)
        {
            try
            {
                if (refid == null) { return RedirectToAction("NotFound", "WIP"); }

                _cvm.referral = await _referralData.GetReferralDetails(refid);
                ICP icp = await _triageData.GetICPDetailsByRefID(refid);
                _cvm.icpCancer = await _triageData.GetCancerICPDetailsByICPID(icp.ICPID);

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "VHRpro-Untested" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewUntestedVHROptions(int refid, int? thresholdAge, bool below30, bool below50, bool other, bool notID)
        {
            try
            {
                int iOther = other ? 1 : 0; //converts bools to ints for the SQL update
                int iNotID = notID ? 1 : 0;

                int success = _crud.CallStoredProcedure("UntestedVHRGroup", "Create", refid, thresholdAge.GetValueOrDefault(), 0, "", "", "", "", User.Identity.Name, null, null, below30, below50, iOther, iNotID);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "VHRPro update untestedgroup" }); }

                bool iSuccess = true;
                string sMessage = "Saved!";
                                
                ICP icp = await _triageData.GetICPDetailsByRefID(refid);
                ICPCancer icpc = await _triageData.GetCancerICPDetailsByICPID(icp.ICPID);

                return RedirectToAction("VHRPro", new { id = icpc.ICP_Cancer_ID, success = iSuccess, message = sMessage });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "VHRpro-Untested" });
            }
        }
    }
}
