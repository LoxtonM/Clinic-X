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
        private readonly DictatedLetterVM lvm;
        private readonly VMData vm;
        private readonly CRUD crud;
        private readonly LetterController lc;


        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            lvm = new DictatedLetterVM();
            vm = new VMData(_clinContext);
            crud = new CRUD(_config);
            lc = new LetterController(_clinContext, _docContext);
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

                var user = vm.GetCurrentStaffUser(User.Identity.Name);
                
                var letters = vm.GetDictatedLettersList(user.STAFF_CODE);

                return View(letters);
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
                lvm.dictatedLetters = vm.GetDictatedLetterDetails(id);
                lvm.dictatedLettersPatients = vm.GetDictatedLettersPatientsList(id);
                lvm.dictatedLettersCopies = vm.GetDictatedLettersCopiesList(id);
                lvm.patients = vm.GetDictatedLetterPatientsList(id);
                lvm.staffMemberList = vm.GetClinicalStaffList();
                int? iMPI = lvm.dictatedLetters.MPI;
                int? iRefID = lvm.dictatedLetters.RefID;
                lvm.patientDetails = vm.GetPatientDetails(iMPI.GetValueOrDefault());
                lvm.activityDetails = vm.GetActivityDetails(iRefID.GetValueOrDefault());
                string sGPCode = lvm.patientDetails.GP_Facility_Code;
                string sRefFacCode = lvm.activityDetails.REF_FAC;
                string sRefPhysCode = lvm.activityDetails.REF_PHYS;
                lvm.referrerFacility = vm.GetFacilityDetails(sRefFacCode);
                lvm.referrer = vm.GetClinicianDetails(sRefPhysCode);
                lvm.GPFacility = vm.GetFacilityDetails(sGPCode);
                lvm.facilities = vm.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                lvm.clinicians = vm.GetClinicianList().Where(f => f.Is_Gp == 0 && f.NAME != null && f.FACILITY != null).ToList();
                lvm.consultants = vm.GetConsultantsList().ToList();
                lvm.gcs = vm.GetGCList().ToList();
                lvm.secteams = vm.GetSecTeamsList();

                return View(lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int iDID, string sStatus, string sLetterTo, string sLetterFrom, string sLetterContent, string sLetterContentBold, 
            bool isAddresseeChanged, string sSecTeam, string sConsultant, string sGC, string sDateDictated)
        {
            try
            {
                DateTime dDateDictated = new DateTime();
                dDateDictated = DateTime.Parse(sDateDictated);
                //two updates required - one to update the addressee (if addressee has changed)
                if (isAddresseeChanged)
                {
                    int iSuccess2 = crud.CallStoredProcedure("Letter", "UpdateAddresses", iDID, 0, 0, "", "", sLetterFrom, sLetterTo, User.Identity.Name);

                    if (iSuccess2 == 0) { return RedirectToAction("Index", "WIP"); }
                }

                int iSuccess = crud.CallStoredProcedure("Letter", "Update", iDID, 0, 0, sStatus, "", sLetterContentBold, sLetterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, sSecTeam, sConsultant, sGC);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        public async Task<IActionResult> Create(int id)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                var dot = await _clinContext.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
                int iDID = dot.DoTID;

                return RedirectToAction("Edit", new { id = iDID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        public async Task<IActionResult> Delete(int iDID)
        {
            try 
            {
                int iSuccess = crud.CallStoredProcedure("Letter", "Delete", iDID, 0, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int iDID)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("Letter", "Approve", iDID, 0, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unapprove(int iDID)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("Letter", "Unapprove", iDID, 0, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPatientToDOT(int iPID, int iDID)
        {
            try
            {                
                int iSuccess = crud.CallStoredProcedure("Letter", "AddFamilyMember", iDID, iPID, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCCToDOT(int iDID, string sCC)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("Letter", "AddCC", iDID, 0, 0, sCC, "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCCFromDOT(int iID)
        {
            try
            {
                //var letter = await _clinContext.DictatedLettersCopies.FirstOrDefaultAsync(x => x.CCID == iID);
                
                var letter = vm.GetDictatedLetterCopyDetails(iID);
                
                int iDID = letter.DotID;

                int iSuccess = crud.CallStoredProcedure("Letter", "DeleteCC", iID, 0, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iDID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        public async Task<IActionResult> PreviewDOT(int iDID)
        {
            try
            {                
                lc.PreviewDOTPDF(iDID, User.Identity.Name, "PT");
                //return RedirectToAction("Edit", new { id = iDID });
                return File("~/preview.pdf", "Application/PDF");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
