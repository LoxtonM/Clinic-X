using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Models;
using ClinicX.ViewModels;
using ClinicX.Meta;
using System.Net.NetworkInformation;

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

                //letters = letters.OrderByDescending(l => l.DateDictated);

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
                lvm.referralDetails = vm.GetReferralDetails(iRefID.GetValueOrDefault());
                string sGPCode = lvm.patientDetails.GP_Facility_Code;
                string sRefFacCode = lvm.referralDetails.ReferringFacilityCode;
                lvm.referrerFacility = vm.GetFacilityDetails(sRefFacCode);
                lvm.GPFacility = vm.GetFacilityDetails(sGPCode);
                lvm.facilities = vm.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                lvm.clinicians = vm.GetClinicianList().Where(f => f.Is_Gp == 0 && f.NAME != null && f.FACILITY != null).ToList();

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
                if(sLetterContentBold == null)
                {
                    sLetterContentBold = "";
                }

                DateTime dDateDictated = new DateTime();

                if (sDateDictated != null)
                {
                    dDateDictated = DateTime.Parse(sDateDictated);
                }
                else
                {
                    dDateDictated = DateTime.Parse("1/1/1900");
                }
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

    }
}
