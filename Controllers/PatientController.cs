using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly PatientVM pvm;
        private readonly VMData vm;

        public PatientController(ClinicalContext context)
        {
            _clinContext = context;
            pvm = new PatientVM();
            vm = new VMData(_clinContext);
        }

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {                
                pvm.patient = vm.GetPatientDetails(id);
                if (pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                pvm.relatives = vm.GetRelativesList(id);
                pvm.hpoTermDetails = vm.GetHPOTermsAddedList(id);
                pvm.referrals = vm.GetReferralsList(id);
                pvm.patientPathway = vm.GetPathwayDetails(id);
                pvm.alerts = vm.GetAlertsList(id);
                pvm.diary = vm.GetDiaryList(id);

                return View(pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        
    }
}
