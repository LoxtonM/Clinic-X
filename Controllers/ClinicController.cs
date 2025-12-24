//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ClinicX.Controllers
{
    public class ClinicController : Controller
    {
       // private readonly ClinicalContext _clinContext;
        private readonly ClinicVM _cvm;
        private readonly IConfiguration _config;        
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly IActivityDataAsync _activityData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IClinicDataAsync _clinicData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IOutcomeDataAsync _outcomeData;
        private readonly IClinicVenueDataAsync _clinicVenueData;
        private readonly IActivityTypeDataAsync _activityTypeData;


        public ClinicController(IConfiguration config, IPatientDataAsync patientData, IReferralDataAsync referralData, IActivityDataAsync activityData, IStaffUserDataAsync staffUserData,
            IClinicDataAsync clinicData, ICRUD crud, IAuditService auditService, IOutcomeDataAsync outcomeData, IClinicVenueDataAsync clinicVenueData, IActivityTypeDataAsync activityTypeData)
        {
            //_clinContext = context;
            _config = config;
            _cvm = new ClinicVM();
            _patientData = patientData;
            _referralData = referralData;
            _activityData = activityData;
            _staffUser = staffUserData;
            _clinicData = clinicData;
            _crud = crud;
            _audit = auditService;
            _outcomeData = outcomeData;
            _clinicVenueData = clinicVenueData;
            _activityTypeData = activityTypeData;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(DateTime? pastFilterDate, bool? isShowOutstanding, DateTime? futureFilterDate)
        {
            try
            {
                _cvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _cvm.staffMember?.STAFF_CODE ?? string.Empty;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinics", "", _ip.GetIPAddress());

                if (pastFilterDate == null) //set default date to 30 days before today
                {
                    pastFilterDate = DateTime.Parse(DateTime.Now.AddDays(-90).ToString());
                }

                if (futureFilterDate == null) //set default date to 30 days before today
                {
                    futureFilterDate = DateTime.Parse(DateTime.Now.AddDays(90).ToString());
                }

                var clinicListAll = await _clinicData.GetClinicList(User.Identity.Name);

                _cvm.pastClinicsList = clinicListAll.Where(c => c.BOOKED_DATE < DateTime.Today).ToList();
                _cvm.currentClinicsList = clinicListAll.Where(c => c.BOOKED_DATE == DateTime.Today).ToList();
                _cvm.futureClinicsList = clinicListAll.Where(c => c.BOOKED_DATE > DateTime.Today).ToList();

                if (isShowOutstanding.GetValueOrDefault())
                {
                    _cvm.pastClinicsList = _cvm.pastClinicsList.Where(c => c.Attendance == "NOT RECORDED").ToList();
                }

                _cvm.pastClinicsList = _cvm.pastClinicsList.Where(c => c.BOOKED_DATE >= pastFilterDate).ToList();
                _cvm.pastClinicsList = _cvm.pastClinicsList.OrderByDescending(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                _cvm.currentClinicsList = _cvm.currentClinicsList.OrderBy(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                _cvm.futureClinicsList = _cvm.futureClinicsList.Where(c => c.BOOKED_DATE <= futureFilterDate).OrderBy(c => c.BOOKED_DATE).ThenBy(c => c.BOOKED_TIME).ToList();
                _cvm.pastClinicFilterDate = pastFilterDate.GetValueOrDefault(); //to allow the HTML to keep selected parameters
                _cvm.futureClinicFilterDate = futureFilterDate.GetValueOrDefault(); //to allow the HTML to keep selected parameters
                _cvm.isClinicOutstanding = isShowOutstanding.GetValueOrDefault();

                return View(_cvm);               
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Clinic" });
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

                _cvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _cvm.staffMember?.STAFF_CODE ?? string.Empty;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinic Details", "RefID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.Clinic = await _clinicData.GetClinicDetails(id);
                _cvm.linkedReferral = await _referralData.GetReferralDetails(_cvm.Clinic.ReferralRefID);

                if(_cvm.Clinic.Attendance.Contains("Att")) //show "seen by" details for completed appts
                {
                    _cvm.seenByString = $"Seen by {_cvm.Clinic.SeenByClinician}";
                    
                    if(_cvm.Clinic.SeenBy2 != null) //add other clinicians if present
                    {
                        _cvm.seenByString = _cvm.seenByString + $", {_cvm.Clinic.SeenByClinician2}";
                    }
                    if (_cvm.Clinic.SeenBy3 != null)
                    {
                        _cvm.seenByString = _cvm.seenByString + $", {_cvm.Clinic.SeenByClinician3}";
                    }

                    _cvm.seenByString = _cvm.seenByString + $" on {_cvm.Clinic.BOOKED_DATE.Value.ToString("dd/MM/yyyy")} at {_cvm.Clinic.ArrivalTime.Value.ToString("HH:mm")}";
                }                


                if (_cvm.Clinic == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Clinic-AppDetails" });
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

                _cvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _cvm.staffMember?.STAFF_CODE ?? string.Empty;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Clinic", "RefID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.staffMembers = await _staffUser.GetClinicalStaffList();
                _cvm.activityItems = await _activityData.GetClinicDetailsList(id);
                _cvm.activityItem = await _activityData.GetActivityDetails(id);
                _cvm.outcomes = await _clinicData.GetOutcomesList();
                _cvm.ethnicities = await _clinicData.GetEthnicitiesList();
                int mpi = _cvm.activityItem.MPI;
                _cvm.patient = await _patientData.GetPatientDetails(mpi);

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formname="Clinic-edit" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int refID, string counseled, string seenBy, DateTime arrivalTime, int noSeen, string letterRequired, bool isClockStop, string? ethnicity, bool? isComplete = false, string? seenBy2="", string? seenBy3="")
        {
            try
            {
                if (refID == null)
                {
                    return NotFound();
                }

                //because it doesn't like passing nulls to SQL, we have to set it to a value SQL can take, then re-set it to null in the SQL
                if (isClockStop == null) { isClockStop = false; }

                if (seenBy == null) { seenBy = ""; }

                if (letterRequired == null) { letterRequired = "No"; }

                if (ethnicity == null) { ethnicity = ""; }
                
                int success = _crud.CallStoredProcedure("Appointment", "Update", refID, noSeen, 0, counseled, seenBy,
                    letterRequired, ethnicity, User.Identity.Name, arrivalTime, null, isClockStop, isComplete, 0,0,0,seenBy2,seenBy3);
                //do the update, return 1 if successful and 0 if not

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName="Clinic-edit(SQL)" }); }

                if (letterRequired != "No")
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "",
                    "", "", User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.",formName = "Clinic-edit(SQL)" }); }
                }

                return RedirectToAction("ApptDetails", new { id = refID });                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Clinic-edit" });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddNew(int mpi)
        {
            _cvm.patient = await _patientData.GetPatientDetails(mpi);
            _cvm.outcomes = await _outcomeData.GetOutcomeList();
            _cvm.staffMembers = await _staffUser.GetClinicalStaffList();
            _cvm.referralsList = await _referralData.GetActiveReferralsListForPatient(mpi);
            _cvm.venueList = await _clinicVenueData.GetVenueList();
            _cvm.appTypeList = await _activityTypeData.GetApptTypes();

            return View(_cvm);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, int linkedRefID, DateTime bookedDate, DateTime bookedTime, string appType, string? outcome, string? venue, string? clinician1,
            string? clinician2, string? clinician3, int? timeSpent, int? noPatientsSeen, string? letterReq, string? counseled, string? callersName, string? callersOrg, string? callersTelNo,
            string? message, string? urgency, bool? isAddAsNote = false, bool? isClockStop = false)
        {
            int refID = 0;

            if (venue == null) { venue = ""; }
            if (clinician1 == null) { clinician1 = _staffUser.GetStaffMemberDetails(User.Identity.Name).Result.STAFF_CODE; }
            if (noPatientsSeen == null) { noPatientsSeen = 1; }

            DateTime bookedTimeEdited = DateTime.Parse("1900-01-01 " + bookedTime.Hour + ":" + bookedTime.Minute + ":" + bookedTime.Second);

            int success = _crud.CallStoredProcedure("Contact", "Create", mpi, linkedRefID, timeSpent.GetValueOrDefault(), appType, venue, clinician1, "", User.Identity.Name,
                bookedDate, bookedTimeEdited, isClockStop, false, noPatientsSeen, 0, 0, clinician2, clinician3, letterReq, 0, 0, 0, 0, 0, counseled);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-create(SQL)" }); }

            List<ActivityItem> appts = await _activityData.GetActivityList(mpi);
            appts = appts.Where(a => a.BOOKED_DATE == bookedDate).OrderByDescending(a => a.RefID).ToList();

            refID = appts.First().RefID;

            if (appType == "Tel. Admin")
            {
                Patient patient = await _patientData.GetPatientDetails(mpi);
                string emailSubject = $"{patient.CGU_No} - {patient.FIRSTNAME} {patient.LASTNAME} - {urgency} Telephone Message";

                string emailMessage = $"Caller - {callersName}%0D%0A%0D%0A" +
                    $"Organisation - {callersOrg}%0D%0A%0D%0A" +
                $"Contact Tel No - {callersTelNo}%0D%0A%0D%0A" +
                message;

                string emailBodyText = "";
                bool isHidden = true;

                if (isAddAsNote.GetValueOrDefault())
                {
                    emailBodyText = "A copy of this message has already been queued for creation in EDMS%0D%0A%0D%0A";
                    isHidden = false;
                }

                _crud.CallStoredProcedure("ClinicalNote", "Create", refID, 0, 0, "", "", "", emailBodyText, User.Identity.Name, null, null, isHidden);

                emailBodyText = emailBodyText + emailMessage;

                return Redirect($"mailto:?subject={emailSubject}&body={emailBodyText}");
            }

            return RedirectToAction("ApptDetails", new { id = refID });
        }
    }
}
