using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ClinicX.Controllers
{
    public class OtherCaseloadController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM cvm;
        private readonly VMData vm;

        public OtherCaseloadController(ClinicalContext context)
        {
            _clinContext = context;
            cvm = new CaseloadVM();
            vm = new VMData(_clinContext);
        }

        [Authorize]
        public IActionResult Index(string? sStaffCode)
        {
            try
            {
                if (sStaffCode == null)
                {
                    sStaffCode = vm.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                }

                cvm.staffCode = sStaffCode;
                cvm.caseLoad = vm.GetCaseloadList(sStaffCode).OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                cvm.clinicians = vm.GetClinicalStaffList();
                if (cvm.caseLoad.Count() > 0)
                {
                    cvm.name = cvm.caseLoad.FirstOrDefault().Clinician;
                }

                return View(cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
