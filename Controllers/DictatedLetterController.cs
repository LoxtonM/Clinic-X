using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;
using System.Security.Cryptography;

namespace ClinicX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly DictatedLetterVM _lvm;
        private readonly VMData _vm;
        private readonly CRUD _crud;
        private readonly LetterController _lc;


        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            _lvm = new DictatedLetterVM();
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
            _lc = new LetterController(_clinContext, _docContext);
            _docContext = docContext;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = _vm.GetCurrentStaffUser(User.Identity.Name);
                
                var letters = _vm.GetDictatedLettersList(user.STAFF_CODE);

                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                return View(_lvm);
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
                _lvm.dictatedLetters = _vm.GetDictatedLetterDetails(id);
                _lvm.dictatedLettersPatients = _vm.GetDictatedLettersPatientsList(id);
                _lvm.dictatedLettersCopies = _vm.GetDictatedLettersCopiesList(id);
                _lvm.patients = _vm.GetDictatedLetterPatientsList(id);
                _lvm.staffMemberList = _vm.GetClinicalStaffList();
                int? iMPI = _lvm.dictatedLetters.MPI;
                int? iRefID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = _vm.GetPatientDetails(iMPI.GetValueOrDefault());
                _lvm.activityDetails = _vm.GetActivityDetails(iRefID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                _lvm.referrerFacility = _vm.GetFacilityDetails(sRefFacCode);
                _lvm.referrer = _vm.GetClinicianDetails(sRefPhysCode);
                _lvm.GPFacility = _vm.GetFacilityDetails(sGPCode);
                _lvm.facilities = _vm.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                _lvm.clinicians = _vm.GetClinicianList().Where(f => f.Is_Gp == 0 && f.NAME != null && f.FACILITY != null).ToList();
                _lvm.consultants = _vm.GetConsultantsList().ToList();
                _lvm.gcs = _vm.GetGCList().ToList();
                _lvm.secteams = _vm.GetSecTeamsList();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int dID, string status, string letterTo, string letterFrom, string letterContent, string letterContentBold, 
            bool isAddresseeChanged, string secTeam, string consultant, string gc, string dateDictated, string letterToCode)
        {
            try
            {
                DateTime dDateDictated = new DateTime();
                dDateDictated = DateTime.Parse(dateDictated);
                //two updates required - one to update the addressee (if addressee has changed)
                if (isAddresseeChanged)
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "UpdateAddresses", dID, 0, 0, "", letterToCode, letterFrom, letterTo, User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("Index", "WIP"); }
                }

                int success = _crud.CallStoredProcedure("Letter", "Update", dID, 0, 0, status, "", letterContentBold, letterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, secTeam, consultant, gc);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Create(int id)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                var dot = await _clinContext.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
                int dID = dot.DoTID;

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Delete(int dID)
        {
            try 
            {
                int success = _crud.CallStoredProcedure("Letter", "Delete", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int dID)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "Approve", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unapprove(int dID)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "Unapprove", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPatientToDOT(int pID, int dID)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Letter", "AddFamilyMember", dID, pID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCCToDOT(int dID, string cc)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "AddCC", dID, 0, 0, cc, "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCCFromDOT(int id)
        {
            try
            {
                //var letter = await _clinContext.DictatedLettersCopies.FirstOrDefaultAsync(x => x.CCID == iID);
                
                var letter = _vm.GetDictatedLetterCopyDetails(id);
                
                int dID = letter.DotID;

                int success = _crud.CallStoredProcedure("Letter", "DeleteCC", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = dID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        public async Task<IActionResult> PreviewDOT(int dID)
        {
            try
            {                
                _lc.PreviewDOTPDF(dID, User.Identity.Name);
                //return RedirectToAction("Edit", new { id = dID });
                return File($"~/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
