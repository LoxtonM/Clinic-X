using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly DictatedLetterVM lvm;
        private readonly VMData vm;
        private readonly CRUD crud;

        public DictatedLetterController(IConfiguration config, ClinicalContext context)
        {
            _context = context;
            _config = config;
            lvm = new DictatedLetterVM();
            vm = new VMData(_context);
            crud = new CRUD(_config);            
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

                var users = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);

                string strStaffCode = users.STAFF_CODE;

                var letters = from l in _context.DictatedLetters
                              where l.LetterFromCode == strStaffCode && l.MPI != null && l.RefID != null && l.Status != "Printed"
                              orderby l.DateDictated descending
                              select l;                

                return View(await letters.ToListAsync());
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
                lvm.dictatedLettersPatients = vm.GetDictatedLettersPatients(id);
                lvm.dictatedLettersCopies = vm.GetDictatedLettersCopies(id);
                lvm.patients = vm.GetPatients(id);
                lvm.staffMemberList = vm.GetClinicians();
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
                lvm.secteams = vm.GetSecTeams();

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
                    crud.CallStoredProcedure("Letter", "UpdateAddresses", iDID, 0, 0, "", "", sLetterFrom, sLetterTo, User.Identity.Name);
                }

                crud.CallStoredProcedure("Letter", "Update", iDID, 0, 0, sStatus, "", sLetterContentBold, sLetterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, sSecTeam, sConsultant, sGC);
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
                crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", "", "", User.Identity.Name);

                var dot = await _context.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
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
                crud.CallStoredProcedure("Letter", "Delete", iDID, 0, 0, "", "", "", "", User.Identity.Name);

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
                crud.CallStoredProcedure("Letter", "Approve", iDID, 0, 0, "", "", "", "", User.Identity.Name);

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
                crud.CallStoredProcedure("Letter", "Unapprove", iDID, 0, 0, "", "", "", "", User.Identity.Name);

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
                crud.CallStoredProcedure("Letter", "AddFamilyMember", iDID, iPID, 0, "", "", "", "", User.Identity.Name);

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
                crud.CallStoredProcedure("Letter", "AddCC", iDID, 0, 0, sCC, "", "", "", User.Identity.Name);
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
                var letter = await _context.DictatedLettersCopies.FirstOrDefaultAsync(x => x.CCID == iID);
                int iDID = letter.DotID;

                crud.CallStoredProcedure("Letter", "DeleteCC", iID, 0, 0, "", "", "", "", User.Identity.Name);
                return RedirectToAction("Edit", new { id = iDID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
