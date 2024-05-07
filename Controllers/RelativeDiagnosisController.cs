using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RelativeDiagnosisController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly RelativeData _relativeData;
        private readonly RelativeDiagnosisData _relativeDiagnosisData;
        private readonly StaffUserData _staffUser;        
        private readonly CRUD _crud;
        private readonly RelativeDiagnosisVM _rdvm;
        public RelativeDiagnosisController(ClinicalContext context, IConfiguration config) 
        {
            _clinContext = context;
            _config = config;                        
            _relativeData = new RelativeData(_clinContext);
            _relativeDiagnosisData = new RelativeDiagnosisData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _crud = new CRUD(_config);
            _rdvm = new RelativeDiagnosisVM();
        }
        public IActionResult Index(int relID)
        {
            try
            {                
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(relID);
                _rdvm.relativesDiagnosisList = _relativeDiagnosisData.GetRelativeDiagnosisList(relID);                
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }            
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.cancerRegList = _relativeDiagnosisData.GetCancerRegList();
                _rdvm.requestStatusList = _relativeDiagnosisData.GetRequestStatusList();     
                _rdvm.staffList = _staffUser.GetStaffMemberList();
                _rdvm.clinicianList = _staffUser.GetClinicalStaffList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int id, string diagnosis, string? age, string? hospital, string? cRegCode, DateTime? dateRequested,
            string? consultant, string? status, string? consent, DateTime? dateReceived)
        {
            try
            {
                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Create", id, 0, 0, diagnosis, age, hospital, cRegCode, User.Identity.Name,
                    dateRequested, DateTime.Parse("1900-01-01"), false, false, 0, 0, 0, status, consent, consultant);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _rdvm.relativesDiagnosis = _relativeDiagnosisData.GetRelativeDiagnosisDetails(id);               
                _rdvm.tumourSiteList = _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = _relativeDiagnosisData.GetTumourMorphList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int tumourID, string? consent="", DateTime? dateReceived=null, string? confirmed="",
            DateTime? confDiagDate=null, string? confDiagAge="", string? siteCode="", string? latCode="", string? grade="", 
            string? dukes="", string? morphCode="", string? histologyNumber="", string? notes="")
        {
            try
            {
                _rdvm.relativesDiagnosis = _relativeDiagnosisData.GetRelativeDiagnosisDetails(tumourID);
                _rdvm.tumourSiteList = _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = _relativeDiagnosisData.GetTumourMorphList();

                string data = "ConfDiagAge:" + confDiagAge + ",Grade:" + grade + ",Dukes:" + dukes + ",HistologyNumber:" + histologyNumber;
               
                //there are too many strings, so I need to concatenate them all to send them to the SP
                //(it's either that or add another 4 optional string variables!!!)

                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Edit", tumourID, 0, 0, consent, confirmed, data, notes, User.Identity.Name, dateReceived, confDiagDate,
                    false, false, 0, 0, 0, siteCode, latCode, morphCode);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                //return View(_rdvm);
                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = tumourID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
