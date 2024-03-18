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
        private readonly VMData vm;
        private readonly CRUD crud;
        private readonly RelativeDiagnosisVM rdvm;
        public RelativeDiagnosisController(ClinicalContext context, IConfiguration config) 
        {
            _clinContext = context;
            _config = config;            
            vm = new VMData(_clinContext);
            crud = new CRUD(_config);
            rdvm = new RelativeDiagnosisVM();
        }
        public IActionResult Index(int iRelID)
        {
            try
            {
                var relDiag = vm.GetRelativeDiagnosisList(iRelID);

                return View(relDiag);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }            
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                rdvm.relativeDetails = vm.GetRelativeDetails(id);
                rdvm.cancerRegList = vm.GetCancerRegList();
                rdvm.requestStatusList = vm.GetRequestStatusList();     
                rdvm.staffList = vm.GetStaffMemberList();
                rdvm.clinicianList = vm.GetClinicalStaffList();

                return View(rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int id, string sDiagnosis, string? sAge, string? sHospital, string? sCRegCode, DateTime? dDateRequested,
            string? sCons, string? sStatus, string? sConsent, DateTime? dDateReceived)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("RelativeDiagnosis", "Create", id, 0, 0, sDiagnosis, sAge, sHospital, sCRegCode, User.Identity.Name,
                    dDateRequested, DateTime.Parse("1900-01-01"), false, false, 0, 0, 0, sStatus, sConsent, sCons);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "RelativeDiagnosis", new { iRelID = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                rdvm.relativesDiagnosis = vm.GetRelativeDiagnosisDetails(id);
                //rdvm.cancerRegList = vm.GetCancerRegList();
                //rdvm.requestStatusList = vm.GetRequestStatusList();
                //rdvm.staffList = vm.GetStaffMemberList();
                //rdvm.clinicianList = vm.GetClinicalStaffList();
                rdvm.tumourSiteList = vm.GetTumourSiteList();
                rdvm.tumourLatList = vm.GetTumourLatList();
                rdvm.tumourMorphList = vm.GetTumourMorphList();

                return View(rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? sConsent, DateTime? dDateReceived, string? sConfirmed,
            DateTime? dConfDiagDate, string? sConfDiagAge, string? sSiteCode, string? sLatCode, string? sGrade, 
            string? sDukes, string? sMorphCode, string? sHistologyNumber, string? sNotes)
        {
            try
            {
                rdvm.relativesDiagnosis = vm.GetRelativeDiagnosisDetails(id);
                rdvm.tumourSiteList = vm.GetTumourSiteList();
                rdvm.tumourLatList = vm.GetTumourLatList();
                rdvm.tumourMorphList = vm.GetTumourMorphList();

                string sData = "ConfDiagAge:" + sConfDiagAge + "Grade:" + sGrade + "Dukes:" + sDukes + "HistologyNumber:" + sHistologyNumber;
                //there are too many strings, so I need to concatenate them all to send them to the SP
                //(it's either that or add another 4 optional string variables!!!)

                int iSuccess = crud.CallStoredProcedure("RelativeDiagnosis", "Edit", id, 0, 0, sConsent, sConfirmed, sData, sNotes, User.Identity.Name, dDateReceived, dConfDiagDate,
                    false, false, 0, 0, 0, sSiteCode, sLatCode, sMorphCode);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                //return View(rdvm);
                return RedirectToAction("Index", "WIP");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
