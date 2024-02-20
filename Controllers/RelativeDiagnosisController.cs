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
            string? sCons, string? sStatus, string? sConsent, DateTime? dDateReceived, string? sReqBy)
        {
            try
            {
                crud.CallStoredProcedure("RelativeDiagnosis", "Create", id, 0, 0, sDiagnosis, sAge, sHospital, sCRegCode, User.Identity.Name,
                    dDateRequested, dDateReceived, false, false, 0, 0, 0, sStatus, sConsent, sReqBy);

                return View();
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
        public async Task<IActionResult> Edit(int id, string sPlaceholder)
        {
            try
            {
                //rdvm.relativesDiagnosis = vm.GetRelativeDiagnosisDetails(id);
                //rdvm.cancerRegList = vm.GetCancerRegList();
                //rdvm.requestStatusList = vm.GetRequestStatusList();
                //rdvm.staffList = vm.GetStaffMemberList();
                //rdvm.clinicianList = vm.GetClinicalStaffList();

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
