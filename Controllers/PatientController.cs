using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Models;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _context;

        public PatientController(ClinicalContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {
                PatientVM pvm = new PatientVM();
                VMData vm = new VMData(_context);
                pvm.patient = vm.GetPatientDetails(id);
                if (pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                pvm.relatives = vm.GetRelativesDetails(id);
                pvm.hpoTermDetails = vm.GetHPOTerms(id);
                pvm.referrals = vm.GetReferrals(id);
                pvm.patientPathway = vm.GetPathwayDetails(id);
                pvm.alerts = vm.GetAlerts(id);

                return View(pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        
    }
}
