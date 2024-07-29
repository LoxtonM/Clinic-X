using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Meta;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;

namespace ClinicX.Controllers
{
    public class RelativeController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly RelativeDiagnosisVM _rdvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public RelativeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _crud = new CRUD(_config);
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _rdvm = new RelativeDiagnosisVM();
            _audit = new AuditService(_config);
        }

        [Authorize]
        public IActionResult Index()
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Relatives");

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - View Relative", "ID=" + id.ToString());
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID).MPI;
                
                return View(_rdvm);
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
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Relative", "ID=" + id.ToString());

                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID).MPI;
                _rdvm.relationList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _relativeData.GetGenderList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string forename1,
            string forename2, string surname, string relation, string dob, string dod,
            int isAffected, string sex)
        {
            try
            {
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);

                //making sure all the nulls have values

                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (dob != null)
                {
                    birthDate = DateTime.Parse(dob);
                }
                else
                {
                    birthDate = DateTime.Parse("1/1/1900");
                }

                if (dod != null)
                {
                    deathDate = DateTime.Parse(dod);
                }
                else
                {
                    deathDate = DateTime.Parse("1/1/1900");
                }

                if (title == null)
                {
                    title = "";
                }

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Edit", id, isAffected, 0, title, forename1, forename2, surname,
                        User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update." }); }

                return RedirectToAction("RelativeDetails", "Relative", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int wmfacsid)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Add Relative", "WMFACSID=" + wmfacsid.ToString());

                _rdvm.WMFACSID = wmfacsid;
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                _rdvm.relationList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _relativeData.GetGenderList();
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int wmfacsid, string title, string forename1, 
            string forename2, string surname, string relation, string sDOB, string sDOD, 
            int isAffected, string sex)
        {
            try
            {
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (sDOB != null)
                {
                    birthDate = DateTime.Parse(sDOB);
                }
                else
                {
                    birthDate = DateTime.Parse("1/1/1900");
                }

                if (sDOD != null)
                {
                    deathDate = DateTime.Parse(sDOD);
                }
                else
                {
                    deathDate = DateTime.Parse("1/1/1900");
                }

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Create", wmfacsid, isAffected, 0, title, forename1, forename2, surname,
                    User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update." }); }

                var patient = _patientData.GetPatientDetailsByWMFACSID(wmfacsid);

                return RedirectToAction("PatientDetails", "Patient", new { id = _rdvm.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
