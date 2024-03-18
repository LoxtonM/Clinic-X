using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ClinicX.Meta;
using ClinicX.Models;

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
        public async Task <IActionResult> Index(DateTime? dFilterDate, bool? isShowOutstanding)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }                

                if (dFilterDate == null) //set default date to 30 days before today
                {
                    dFilterDate = DateTime.Parse(DateTime.Now.AddDays(-30).ToString());
                }
                                
                cvm.pastClinicsList = vm.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE < DateTime.Today).ToList();
                cvm.currentClinicsList = vm.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE == DateTime.Today).ToList();
                cvm.futureClinicsList = vm.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE > DateTime.Today).ToList();

                if (isShowOutstanding.GetValueOrDefault())
                {
                    cvm.pastClinicsList = cvm.pastClinicsList.Where(c => c.Attendance == "NOT RECORDED").ToList();
                }

                cvm.pastClinicsList = cvm.pastClinicsList.Where(c => c.BOOKED_DATE >= dFilterDate).ToList();
                cvm.pastClinicsList = cvm.pastClinicsList.OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                cvm.dClinicFilterDate = dFilterDate.GetValueOrDefault(); //to allow the HTML to keep selected parameters
                cvm.isClinicOutstanding = isShowOutstanding.GetValueOrDefault();

                return View(cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ApptDetails(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

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
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                cvm.staffMembers = vm.GetClinicalStaffList();
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
                
                int iSuccess = crud.CallStoredProcedure("Appointment", "Update", iRefID, iNoSeen, 0, sCounseled, sSeenBy,
                    sLetterRequired, sEthnicity, User.Identity.Name, dArrivalTime, null, isClockStop, isComplete);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                if (sLetterRequired != "No")
                {
                    int iSuccess2 = crud.CallStoredProcedure("Letter", "Create", 0, iRefID, 0, "", "",
                    "", "", User.Identity.Name);

                    if (iSuccess2 == 0) { return RedirectToAction("Index", "WIP"); }
                }

                return RedirectToAction("ApptDetails", new { id = iRefID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        
    }
}
