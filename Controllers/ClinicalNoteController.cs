using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.Data;

namespace ClinicX.Controllers
{
    public class ClinicalNoteController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        private readonly ClinicalNoteVM _cvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IActivityData _activityData;
        private readonly IClinicalNoteData _clinicalNoteData;        
        private readonly IClinicData _clinicData;
        private readonly IReferralData _referralData;
        private readonly IMiscData _misc;        
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public ClinicalNoteController(IConfiguration config, IStaffUserData staffUserData, IPatientData patientData, IActivityData activityData, IClinicalNoteData clinicalNoteData,
            IClinicData clinicData, IReferralData referralData, ICRUD crud, IMiscData miscData, IAuditService auditService)
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            _config = config;
            //_staffUser = new StaffUserData(_clinContext);
            _staffUser = staffUserData;
            //_patientData = new PatientData(_clinContext);
            _patientData = patientData;
            //_activityData = new ActivityData(_clinContext);
            _activityData = activityData;
            //_clinicalNoteData = new ClinicalNoteData(_cXContext);
            _clinicalNoteData = clinicalNoteData;
            //_clinicData = new ClinicData(_clinContext);
            _clinicData = clinicData;
            //_referralData = new ReferralData(_clinContext);
            _referralData = referralData;
            //_crud = new CRUD(_config);
            _crud = crud;
            _cvm = new ClinicalNoteVM();
            //_misc = new MiscData(_config);
            _misc = miscData;
            //_audit = new AuditService(_config);
            _audit = auditService;
        }       
        
        [Authorize]
        public IActionResult Index(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinical Notes", "", _ip.GetIPAddress());

                _cvm.clinicalNotesList = _clinicalNoteData.GetClinicalNoteList(id);
                _cvm.patient = _patientData.GetPatientDetails(id);
                _cvm.noteCount = _cvm.clinicalNotesList.Count();                

                return View(_cvm);
            }
            catch(Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="ClinicalNotes" });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Create Clinical Note", "ClinicalNoteID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.activityItem = _activityData.GetActivityDetails(id);
                _cvm.noteTypeList = _clinicalNoteData.GetNoteTypesList();

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNote-create" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int mpi, int refID, string noteType, string clinicalNote)
        {
            try
            {                                
                int noteID;

                int success = _crud.CallStoredProcedure("Clinical Note", "Create", mpi, refID, 0, noteType, "", "",
                    clinicalNote, User.Identity.Name);
                //do the update, return 1 if successful and 0 if not
                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName="ClinicalNote-create(SQL)" }); }

                noteID = _misc.GetClinicalNoteID(refID); //get the newly created ClinicalNoteID so we can redirect to it

                return RedirectToAction("Edit", new { id = noteID });                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNote-create" });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Clinical Note", "ClinicalNoteID=" + id.ToString(), _ip.GetIPAddress());

                _cvm.clinicalNote = _clinicalNoteData.GetClinicalNoteDetails(id);
                _cvm.patient = _patientData.GetPatientDetails(_cvm.clinicalNote.MPI.GetValueOrDefault());

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNote-edit" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(int noteID, string clinicalNote)
        {
            try
            {
                if (noteID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                int success = _crud.CallStoredProcedure("Clinical Note", "Update", noteID, 0, 0, 
                    "", "", "", clinicalNote, User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName="ClinicalNote-edit(SQL)" }); }

                return RedirectToAction("Edit", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ClinicalNote-edit" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChooseAppt(int id) //before we create a note, we have to link it to either an appointment or a referral
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinical Notes Choose Appt", id.ToString(), _ip.GetIPAddress());

                _cvm.patient = _patientData.GetPatientDetails(id);
                _cvm.Clinics = _clinicData.GetClinicByPatientsList(id);
                _cvm.Referrals = _referralData.GetActiveReferralsListForPatient(id);

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> Finalise(int id) //sets a clinical note to "final" so it appears in CGU_DB as complete
        {
            try
            {
                int success = _crud.CallStoredProcedure("Clinical Note", "Finalise", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update." }); }

                //var note = await _cXContext.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);
                var note = _clinicalNoteData.GetClinicalNoteDetails(id);

                return RedirectToAction("Index", new { id = note.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        

    }
}
