using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class ClinicController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly ClinicVM cvm;
        private readonly VMData vm;
        private readonly CRUD crud;

        public ClinicController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            cvm = new ClinicVM();
            vm = new VMData(_clinContext);
            crud = new CRUD(_config);
        }

        [HttpGet]
        [Authorize]
        public async Task <IActionResult> Index(DateTime? dFilterDate)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                

                if (dFilterDate == null)
                {
                    dFilterDate = DateTime.Parse(DateTime.Now.AddDays(-30).ToString());
                }

                var clinics = vm.GetClinicList(User.Identity.Name);
                clinics = clinics.Where(c => c.BOOKED_DATE >= dFilterDate).ToList();
                clinics = clinics.OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();

                return View(clinics);
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
                var appts = vm.GetClinicDetails(id);

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
                cvm.staffMembers = vm.GetCliniciansList();
                cvm.activityItems = vm.GetClinicDetailsList(id);
                cvm.activityItem = vm.GetActivityDetails(id);
                cvm.outcomes = vm.GetOutcomesList();
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
                    sLetterRequired = "No";
                }

                if (sEthnicity == null)
                {
                    sEthnicity = "";
                }
                
                crud.CallStoredProcedure("Appointment", "Update", iRefID, iNoSeen, 0, sCounseled, sSeenBy,
                    sLetterRequired, sEthnicity, User.Identity.Name, dArrivalTime, null, isClockStop, isComplete);

                if(sLetterRequired != "No")
                {
                    crud.CallStoredProcedure("Letter", "Create", 0, iRefID, 0, "", "",
                    "", "", User.Identity.Name);
                }

                return RedirectToAction("ApptDetails", new { id = iRefID });
                //}
                //return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        
    }
}
