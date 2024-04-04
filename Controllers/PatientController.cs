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
        private readonly PatientVM _pvm;
        private readonly VMData _vm;

        public PatientController(ClinicalContext context)
        {
            _clinContext = context;
            _pvm = new PatientVM();
            _vm = new VMData(_clinContext);
        }

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {                
                _pvm.patient = _vm.GetPatientDetails(id);
                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                _pvm.relatives = _vm.GetRelativesList(id);
                _pvm.hpoTermDetails = _vm.GetHPOTermsAddedList(id);
                _pvm.referrals = _vm.GetReferralsList(id);
                _pvm.patientPathway = _vm.GetPathwayDetails(id);
                _pvm.alerts = _vm.GetAlertsList(id);
                _pvm.diary = _vm.GetDiaryList(id);

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        
    }
}
