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
        private readonly CaseloadVM _cvm;
        private readonly VMData _vm;

        public OtherCaseloadController(ClinicalContext context)
        {
            _clinContext = context;
            _cvm = new CaseloadVM();
            _vm = new VMData(_clinContext);
        }

        [Authorize]
        public IActionResult Index(string? staffCode)
        {
            try
            {
                if (staffCode == null)
                {
                    staffCode = _vm.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                }

                _cvm.staffCode = staffCode;
                _cvm.caseLoad = _vm.GetCaseloadList(staffCode).OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                _cvm.clinicians = _vm.GetClinicalStaffList();
                if (_cvm.caseLoad.Count() > 0)
                {
                    _cvm.name = _cvm.caseLoad.FirstOrDefault().Clinician;
                }

                return View(_cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
