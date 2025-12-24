//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
//using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RelativeDiagnosisController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        private readonly RelativeDiagnosisVM _rdvm;
        private readonly IConfiguration _config;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IRelativeDiagnosisDataAsync _relativeDiagnosisData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        
        public RelativeDiagnosisController(IConfiguration config, IRelativeDataAsync relativeData, IRelativeDiagnosisDataAsync relativeDiagnosisData, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData,
            ICRUD crud, IAuditService auditService) 
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            _config = config;                        
            _relativeData = relativeData;
            _relativeDiagnosisData = relativeDiagnosisData;
            _staffUser = staffUserData;
            _patientData = patientData;
            _crud = crud;
            _rdvm = new RelativeDiagnosisVM();
            _audit = auditService;
        }

        [Authorize]
        public async Task<IActionResult> Index(int relID)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Relative Diagnoses", "ID=" + relID.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(relID);
                var pat = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                _rdvm.MPI = pat.MPI;
                _rdvm.relativesDiagnosisList = await _relativeDiagnosisData.GetRelativeDiagnosisList(relID);                
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis" });
            }            
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Add Relative Diagnosis", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(id);
                _rdvm.cancerRegList = await _relativeDiagnosisData.GetCancerRegList();
                _rdvm.requestStatusList = await _relativeDiagnosisData.GetRequestStatusList();     
                _rdvm.staffList = await _staffUser.GetStaffMemberList();
                _rdvm.clinicianList = await _staffUser.GetClinicalStaffList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-add" });
            }
        }

        [HttpPost]
        public IActionResult AddNew(int id, string diagnosis, string? age, string? hospital, string? cRegCode, DateTime? dateRequested,
            string? consultant, string? status, string? consent, DateTime? dateReceived)
        {
            try
            {
                if (cRegCode == null) { cRegCode = ""; }
                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Create", id, 0, 0, diagnosis, age, cRegCode, hospital, User.Identity.Name,
                    dateRequested, DateTime.Parse("1900-01-01"), false, false, 0, 0, 0, status, consent, consultant);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiagnosis-add(SQL)" }); }

                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-add" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Relative Diagnosis", "ID=" + id.ToString());

                _rdvm.relativesDiagnosis = await _relativeDiagnosisData.GetRelativeDiagnosisDetails(id);               
                _rdvm.tumourSiteList = await _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = await _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = await _relativeDiagnosisData.GetTumourMorphList();                

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int tumourID, string? consent="", DateTime? dateReceived=null, string? confirmed="",
            DateTime? confDiagDate=null, string? confDiagAge="", string? siteCode="", string? latCode="", string? grade="", 
            string? dukes="", string? morphCode="", string? histologyNumber="", string? notes="")
        {
            try
            {
                _rdvm.relativesDiagnosis = await _relativeDiagnosisData.GetRelativeDiagnosisDetails(tumourID);
                _rdvm.tumourSiteList = await _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = await _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = await _relativeDiagnosisData.GetTumourMorphList();

                string data = "ConfDiagAge:" + confDiagAge + ",Grade:" + grade + ",Dukes:" + dukes + ",HistologyNumber:" + histologyNumber;               
                //there are too many strings, so I need to concatenate them all to send them to the SP
                //(it's either that or add another 4 optional string variables - a limitation of my chosen method!)

                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Edit", tumourID, 0, 0, consent, confirmed, data, notes, User.Identity.Name, dateReceived, confDiagDate,
                    false, false, 0, 0, 0, siteCode, latCode, morphCode);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiagnosis-edit(SQL)" }); }
                                
                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = tumourID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-edit" });
            }
        }
    }
}
