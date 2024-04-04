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
        private readonly VMData _vm;
        private readonly CRUD _crud;
        private readonly RelativeDiagnosisVM _rdvm;
        public RelativeDiagnosisController(ClinicalContext context, IConfiguration config) 
        {
            _clinContext = context;
            _config = config;            
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
            _rdvm = new RelativeDiagnosisVM();
        }
        public IActionResult Index(int relID)
        {
            try
            {
                var relDiag = _vm.GetRelativeDiagnosisList(relID);

                return View(relDiag);
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
                _rdvm.relativeDetails = _vm.GetRelativeDetails(id);
                _rdvm.cancerRegList = _vm.GetCancerRegList();
                _rdvm.requestStatusList = _vm.GetRequestStatusList();     
                _rdvm.staffList = _vm.GetStaffMemberList();
                _rdvm.clinicianList = _vm.GetClinicalStaffList();

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
                _rdvm.relativesDiagnosis = _vm.GetRelativeDiagnosisDetails(id);
                //_rdvm.cancerRegList = _vm.GetCancerRegList();
                //_rdvm.requestStatusList = _vm.GetRequestStatusList();
                //_rdvm.staffList = _vm.GetStaffMemberList();
                //_rdvm.clinicianList = _vm.GetClinicalStaffList();
                _rdvm.tumourSiteList = _vm.GetTumourSiteList();
                _rdvm.tumourLatList = _vm.GetTumourLatList();
                _rdvm.tumourMorphList = _vm.GetTumourMorphList();

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
                _rdvm.relativesDiagnosis = _vm.GetRelativeDiagnosisDetails(tumourID);
                _rdvm.tumourSiteList = _vm.GetTumourSiteList();
                _rdvm.tumourLatList = _vm.GetTumourLatList();
                _rdvm.tumourMorphList = _vm.GetTumourMorphList();

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
