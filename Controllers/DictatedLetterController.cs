using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IConstantsData _constantsData;

        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext, ClinicXContext cXContext)
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
            _constantsData = new ConstantsData(_docContext);
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
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Letters", "", _ip.GetIPAddress());

                var letters = _dictatedLetterData.GetDictatedLettersList(user.STAFF_CODE);

                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="DictatedLetter" });
            }
        }

        [Authorize]
        public async Task<IActionResult> OtherCliniciansLetters(string? staffCode)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Letters", "", _ip.GetIPAddress());

                _lvm.staffMemberList = _staffUser.GetClinicalStaffList();

                if(staffCode == null) {  staffCode = user.STAFF_CODE; }//default to logged on user

                _lvm.staffUser = _staffUser.GetStaffMemberDetailsByStaffCode(staffCode);

                var letters = _dictatedLetterData.GetDictatedLettersList(staffCode);

                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [Authorize]
        public async Task<IActionResult> DictatedLettersForPatient(int id)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "ClinicX - Letters", "", _ip.GetIPAddress());

                _lvm.patientDetails = _patientData.GetPatientDetails(id);
                var letters = _dictatedLetterData.GetDictatedLettersForPatient(id);

                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {            
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Letter", "ID=" + id.ToString(), _ip.GetIPAddress());                
                _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(id);
                
                _lvm.dictatedLettersCopies = _dictatedLetterData.GetDictatedLettersCopiesList(id);
                _lvm.patients = _dictatedLetterData.GetDictatedLetterPatientsList(id);
                List<DictatedLettersPatient> dlp = new List<DictatedLettersPatient>();
                dlp = _dictatedLetterData.GetDictatedLettersPatientsList(id).ToList();
                _lvm.dictatedLettersPatients = new List<Patient>();
                
                foreach(var p in dlp) //to add additional family members to the "Re:" list
                {
                    Patient pat = _patientData.GetPatientDetails(p.MPI);
                    _lvm.dictatedLettersPatients.Add(pat);
                    _lvm.patients.Remove(pat);
                }

                _lvm.staffMemberList = _staffUser.GetClinicalStaffList();
                int? mpi = _lvm.dictatedLetters.MPI;
                int? refID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = _patientData.GetPatientDetails(mpi.GetValueOrDefault());
                _lvm.activityDetails = _activityData.GetActivityDetails(refID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                if (sGPCode == null ) { sGPCode = "Unknown1"; } //because obviously there are people with nulls in the GP field.
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                if (sRefFacCode == null) { sRefFacCode = "Unknown"; } 
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                if (sRefPhysCode == null) { sRefPhysCode = "Unknown"; }
                _lvm.referrerFacility = _externalFacilityData.GetFacilityDetails(sRefFacCode);                
                _lvm.referrer = _externalClinicianData.GetClinicianDetails(sRefPhysCode);                
                _lvm.GPFacility = _externalFacilityData.GetFacilityDetails(sGPCode);
                _lvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList(); 
                //including all clinicians will slow it down too much, so I'm taking a leap of logic here and assuming one wouldn't want to 
                //write to a GP other than the one registered for the patient (or the one who did the referral).
                _lvm.clinicians = _externalClinicianData.GetClinicianList().Where(c => c.Is_GP == 0 && c.LAST_NAME != null && c.FACILITY != null).ToList();
                List<ExternalCliniciansAndFacilities> extClins = _lvm.clinicians.Where(c => c.POSITION != null).ToList();                
                _lvm.consultants = _staffUser.GetConsultantsList().ToList();
                _lvm.gcs = _staffUser.GetGCList().ToList();
                _lvm.secteams = _staffUser.GetSecTeamsList();
                _lvm.specialities = _externalClinicianData.GetClinicianTypeList();
                _lvm.edmsLink = _constantsData.GetConstant("GEMRLink", 1); //set web link to EDMS

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int dID, string status, string letterTo, string letterFromCode, string letterContent, string letterContentBold, 
            bool isAddresseeChanged, string secTeam, string consultant, string gc, string dateDictated, string letterToCode, string enclosures, string comments,
            string salutation, string? ccAddress, bool? doPreview)
        {
            try
            {
                DateTime dDateDictated = new DateTime();

                if (dateDictated != null) { dDateDictated = DateTime.Parse(dateDictated); }
                else { dDateDictated = DateTime.Parse("1900-01-01"); } //set a default if no value

                //two updates required - one to update the addressee (if addressee has changed)
                if (isAddresseeChanged)
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "UpdateAddresses", dID, 0, 0, salutation, letterToCode, letterFromCode, letterTo, User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }
                }

                if(enclosures == null) { enclosures = ""; } //more potential nulls that have to be set to default values
                if(letterContentBold == null) { letterContentBold = ""; }
                if(letterContent == null) { letterContent = ""; }

                int success = _crud.CallStoredProcedure("Letter", "Update", dID, 0, 0, status, enclosures, letterContentBold, letterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, secTeam, consultant, gc, 0,0,0,0,0, comments, salutation);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }

                if(ccAddress != null)
                {
                    return RedirectToAction("AddCCToDOT", new { dID = dID, cc = ccAddress });
                }

                if(doPreview.GetValueOrDefault())
                {
                    return RedirectToAction("PreviewDOT", new { dID = dID });
                }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [Authorize]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                int success = _crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", staffCode, "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }
                                
                List<DictatedLetter> dotList = _dictatedLetterData.GetDictatedLettersList(staffCode).Where(l => l.RefID == id).OrderByDescending(l => l.CreatedDate).ToList();
                DictatedLetter dot = dotList.First(); //SHOULD get the one you just did...
                int dID = dot.DoTID;
                int mpi = _dictatedLetterData.GetDictatedLetterDetails(dID).MPI.GetValueOrDefault();

                int success2 = _crud.CallStoredProcedure("Letter", "AddFamilyMember", dID, mpi, 0, "", "", "", "", User.Identity.Name); //add the patient to the DOT

                if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addPt(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }

        [Authorize]
        public async Task<IActionResult> Delete(int dID)
        {
            try 
            {
                int success = _crud.CallStoredProcedure("Letter", "Delete", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int dID, bool? isCloseReferral=false)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Letter", "Approve", dID, 0, 0, "", "", "", "", User.Identity.Name, null, null, isCloseReferral);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-approve(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-approve" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unapprove(int dID)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "Unapprove", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-unapprove(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-unapprove" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPatientToDOT(int pID, int dID)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Letter", "AddFamilyMember", dID, pID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addPt(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addPt" });
            }
        }

        [Authorize]
        public async Task<IActionResult> RemovePatientsFromDOT(int dotID)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Letter", "RemoveFamMembers", dotID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-removePts(SQL)" }); }

                return RedirectToAction("Edit", new { id = dotID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-removePts" });
            }
        }

        //[HttpPost]
        public async Task<IActionResult> AddCCToDOT(int dID, string cc)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Letter", "AddCC", dID, 0, 0, "", "", "", cc, User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addCC" });
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

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-deleteCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-deleteCC" });
            }
        }

        [Authorize]
        public async Task<IActionResult> PreviewDOT(int dID)
        {
            try
            {
                _lc.PrintDOTPDF(dID, User.Identity.Name, true);
                //LetterControllerLOCAL lc = new LetterControllerLOCAL(_clinContext, _docContext); //for testing purposes
                //lc.PrintDOTPDF(dID, User.Identity.Name, true); //FOR TESTING ONLY - production should use the data library instead
                
                return File($"~/DOTLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            }
        }

        [Authorize]
        public async Task<IActionResult> ActivityItems(int id)
        {
            try
            {
                _lvm.patientDetails = _patientData.GetPatientDetails(id);
                _lvm.activities = _activityData.GetActivityList(id);
                
                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> NewDOTLetterPatient()
        {
            try
            {
                _lvm.patientList = new List<Patient>();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewDOTLetterPatient(string cguNo)
        {
            try
            {
                _lvm.patientList = _patientData.GetPatientsInPedigree(cguNo);
                
                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchClinician(int dotID)
        {
            _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(dotID);
            _lvm.clinicians = new List<ExternalCliniciansAndFacilities>(); //because we have to have something or it throws a fit

            return View(_lvm);
        }

        [HttpPost]
        public async Task<IActionResult> SearchClinician(int dotID, string firstName, string lastName, string hospitalName, string speciality, string? addressToAdd, bool isSearchOnly)
        {
            _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(dotID);
            _lvm.clinicians = _externalClinicianData.GetClinicianList();            

            if (firstName != null)
            {
                _lvm.clinicians = _lvm.clinicians.Where(c => c.FIRST_NAME != null).ToList(); //because of course there are nulls.
                _lvm.clinicians = _lvm.clinicians.Where(c => c.FIRST_NAME.ToUpper() == firstName.ToUpper()).ToList();
            }

            if (lastName != null)
            {
                _lvm.clinicians = _lvm.clinicians.Where(c => c.LAST_NAME != null).ToList();
                _lvm.clinicians = _lvm.clinicians.Where(c => c.LAST_NAME.ToUpper() == lastName.ToUpper()).ToList();
            }

            if (hospitalName != null)
            {
                _lvm.clinicians = _lvm.clinicians.Where(c => c.FACILITY != null).ToList(); 
                _lvm.clinicians = _lvm.clinicians.Where(c => c.FACILITY.ToUpper().Contains(hospitalName.ToUpper())).ToList();
            }

            if (speciality != null)
            {                
                _lvm.clinicians = _lvm.clinicians.Where(c => c.SPECIALITY.ToUpper() == speciality.ToUpper() || c.POSITION.ToUpper().Contains(speciality.ToUpper())).ToList();
            }

            if (!isSearchOnly)
            {
                int success = _crud.CallStoredProcedure("Letter", "AddCC", dotID, 0, 0, "", "", "", addressToAdd, User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addCC(SQL)" }); }
                
            }

            return View(_lvm);
        }
    }
}
