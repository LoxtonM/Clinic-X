using Microsoft.AspNetCore.Mvc;
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
        private readonly ClinicVM _cvm;
        private readonly IConfiguration _config;        
        private readonly IPatientData _patientData;
        private readonly IReferralData _referralData;
        private readonly IActivityData _activityData;
        private readonly IStaffUserData _staffUser;
        private readonly IClinicData _clinicData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        
        public ClinicController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _cvm = new ClinicVM();
            _patientData = new PatientData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _clinicData = new ClinicData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }


        [HttpGet]
        [Authorize]
        public async Task <IActionResult> Index(DateTime? filterDate, bool? isShowOutstanding)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                else
                {
                    string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                    _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinics");

                if (filterDate == null) //set default date to 30 days before today
                    {
                        filterDate = DateTime.Parse(DateTime.Now.AddDays(-90).ToString());
                    }

                    _cvm.pastClinicsList = _clinicData.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE < DateTime.Today).ToList();
                    _cvm.currentClinicsList = _clinicData.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE == DateTime.Today).ToList();
                    _cvm.futureClinicsList = _clinicData.GetClinicList(User.Identity.Name).Where(c => c.BOOKED_DATE > DateTime.Today).ToList();

                    if (isShowOutstanding.GetValueOrDefault())
                    {
                        _cvm.pastClinicsList = _cvm.pastClinicsList.Where(c => c.Attendance == "NOT RECORDED").ToList();
                    }

                    _cvm.pastClinicsList = _cvm.pastClinicsList.Where(c => c.BOOKED_DATE >= filterDate).ToList();
                    _cvm.pastClinicsList = _cvm.pastClinicsList.OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                    _cvm.currentClinicsList = _cvm.currentClinicsList.OrderBy(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                    _cvm.futureClinicsList = _cvm.futureClinicsList.OrderBy(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                    _cvm.clinicFilterDate = filterDate.GetValueOrDefault(); //to allow the HTML to keep selected parameters
                    _cvm.isClinicOutstanding = isShowOutstanding.GetValueOrDefault();

                    return View(_cvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
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

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinic Details", "RefID=" + id.ToString());

                _cvm.Clinic = _clinicData.GetClinicDetails(id);
                _cvm.linkedReferral = _referralData.GetReferralDetails(_cvm.Clinic.ReferralRefID);

                if (_cvm.Clinic == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
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

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Clinic", "RefID=" + id.ToString());

                _cvm.staffMembers = _staffUser.GetClinicalStaffList();
                _cvm.activityItems = _activityData.GetClinicDetailsList(id);
                _cvm.activityItem = _activityData.GetActivityDetails(id);
                _cvm.outcomes = _clinicData.GetOutcomesList();
                _cvm.ethnicities = _clinicData.GetEthnicitiesList();
                int mpi = _cvm.activityItem.MPI;
                _cvm.patients = _patientData.GetPatientDetails(mpi);

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int refID, string counseled, string seenBy, DateTime arrivalTime, int noSeen, string letterRequired, bool isClockStop, string? ethnicity, bool? isComplete = false)
        {
            try
            {
                if (refID == null)
                {
                    return NotFound();
                }

                //because it doesn't like passing nulls to SQL
                if (isClockStop == null) { isClockStop = false; }

                if (seenBy == null)
                {
                    seenBy = "";
                }

                if (letterRequired == null)
                {
                    letterRequired = "No";
                }

                if (ethnicity == null)
                {
                    ethnicity = "";
                }
                
                int success = _crud.CallStoredProcedure("Appointment", "Update", refID, noSeen, 0, counseled, seenBy,
                    letterRequired, ethnicity, User.Identity.Name, arrivalTime, null, isClockStop, isComplete);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update." }); }

                if (letterRequired != "No")
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "",
                    "", "", User.Identity.Name);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update." }); }
                }

                return RedirectToAction("ApptDetails", new { id = refID });                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        
    }
}
