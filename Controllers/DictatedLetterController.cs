using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;
using ClinicX.Models;

namespace ClinicX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly LetterController _lc;
        private readonly DictatedLetterVM _lvm;
        private readonly IConfiguration _config;        
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IActivityData _activityData;
        private readonly IDictatedLetterData _dictatedLetterData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IExternalFacilityData _externalFacilityData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            _lvm = new DictatedLetterVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _dictatedLetterData = new DictatedLetterData(_clinContext);
            _externalClinicianData = new ExternalClinicianData(_clinContext);
            _externalFacilityData = new ExternalFacilityData(_clinContext);
            _crud = new CRUD(_config);
            _lc = new LetterController(_clinContext, _docContext);
            _audit = new AuditService(_config);
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

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Letters");

                var letters = _dictatedLetterData.GetDictatedLettersList(user.STAFF_CODE);

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Letter", "ID=" + id.ToString());

                _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(id);
                _lvm.dictatedLettersPatients = _dictatedLetterData.GetDictatedLettersPatientsList(id);
                _lvm.dictatedLettersCopies = _dictatedLetterData.GetDictatedLettersCopiesList(id);
                _lvm.patients = _dictatedLetterData.GetDictatedLetterPatientsList(id);
                _lvm.staffMemberList = _staffUser.GetClinicalStaffList();
                int? mpi = _lvm.dictatedLetters.MPI;
                int? refID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = _patientData.GetPatientDetails(mpi.GetValueOrDefault());
                _lvm.activityDetails = _activityData.GetActivityDetails(refID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                _lvm.referrerFacility = _externalFacilityData.GetFacilityDetails(sRefFacCode);
                _lvm.referrer = _externalClinicianData.GetClinicianDetails(sRefPhysCode);
                _lvm.GPFacility = _externalFacilityData.GetFacilityDetails(sGPCode);
                _lvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                _lvm.clinicians = _externalClinicianData.GetClinicianList().Where(c => c.Is_Gp == 0 && c.NAME != null && c.FACILITY != null).ToList();
                List<ExternalClinician> extClins = _lvm.clinicians.Where(c => c.POSITION != null).ToList();
                _lvm.cardio = extClins.Where(c => c.POSITION.Contains("Cardio")).ToList();
                _lvm.genetics = extClins.Where(c => c.POSITION.Contains("Genetic")).ToList();
                _lvm.gynae = extClins.Where(c => c.POSITION.Contains("Gyna")).ToList();
                _lvm.histo = extClins.Where(c => c.POSITION.Contains("Histo")).ToList();
                _lvm.screeningco = extClins.Where(c => c.POSITION.Contains("Screening")).ToList();
                //_lvm.gps_all = extClins.Where(c => c.Is_Gp != 0).ToList(); this refuses to work, for some reason!!!
                _lvm.consultants = _staffUser.GetConsultantsList().ToList();
                _lvm.gcs = _staffUser.GetGCList().ToList();
                _lvm.secteams = _staffUser.GetSecTeamsList();

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
        public async Task<IActionResult> Approve(int dID, bool? isCloseReferral=false)
        {
            try
            {
                
                int success = _crud.CallStoredProcedure("Letter", "Approve", dID, 0, 0, "", "", "", "", User.Identity.Name, null, null, isCloseReferral);

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
                var letter = _dictatedLetterData.GetDictatedLetterCopyDetails(id);
                
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
