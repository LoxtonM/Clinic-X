using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class ClinicController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;

        public ClinicController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize]
        public async Task <IActionResult> Index()
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                var users = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);

                string strStaffCode = users.STAFF_CODE;

                var clinics = from c in _context.Clinics
                              where c.AppType.Contains("App") && c.STAFF_CODE_1 == strStaffCode && c.Attendance != "Declined"
                              orderby c.BOOKED_DATE descending
                              select c;

                return View(await clinics.ToListAsync());
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]        
        public async Task<IActionResult> ApptDetails(int id)
        {
            try
            {
                var appts = await _context.Clinics.FirstOrDefaultAsync(a => a.RefID == id);
                if (appts == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(appts);
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
                ClinicVM cvm = new ClinicVM();
                VMData vm = new VMData(_context);
                cvm.staffMembers = vm.GetStaffMember();
                cvm.activityItems = vm.GetClinicDetailsList(id);
                cvm.activityItem = vm.GetClinicDetails(id);
                cvm.outcomes = vm.GetOutcomes();
                cvm.ethnicities = vm.GetEthnicitiesList();
                int iMPI = cvm.activityItem.MPI;
                cvm.patients = vm.GetPatientDetails(iMPI);

                return View(cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int iRefID, string sCounseled, string sSeenBy, DateTime dArrivalTime, int iNoSeen, string sLetterRequired, bool isClockStop, string? sEthnicity, bool? isComplete = false)
        {
            try
            {
                if (iRefID == null)
                {
                    return NotFound();
                }

                if (isClockStop == null) { isClockStop = false; }

                if (sSeenBy == null)
                {
                    sSeenBy = "";
                }

                if (sLetterRequired == null)
                {
                    sLetterRequired = "";
                }

                if (sEthnicity == null)
                {
                    sEthnicity = "";
                }

                CRUD crud = new CRUD(_config);
                crud.CallStoredProcedure("Appointment", "Update", iRefID, iNoSeen, 0, sCounseled, sSeenBy,
                    sLetterRequired, sEthnicity, User.Identity.Name, dArrivalTime, null, isClockStop, isComplete);

                return RedirectToAction("ApptDetails", new { id = iRefID });
                //}
                //return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }       

        //private bool clinicExists(int id)
        //{
        //    return _context.ActivityItems.Any(c => c.RefID == id);
        //}
    }
}
