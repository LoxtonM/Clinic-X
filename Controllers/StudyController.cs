using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class StudyController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IStudyDataAsync _studyData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IAuditService _audit;
        private readonly ICRUD _crud;
        private readonly StudyVM _svm;

        public StudyController(IConfiguration config, IStudyDataAsync studyData, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData, IAuditService audit, ICRUD crud)
        {
            _config = config;
            _studyData = studyData;
            _svm = new StudyVM();
            _staffUser = staffUserData;
            _crud = crud;
            _patientData = patientData;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int mpi, string message, bool success)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Study List", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

            _svm.studies = await _studyData.GetStudiesForPatient(mpi);
            _svm.patient = await _patientData.GetPatientDetails(mpi);

            _svm.message = message;
            _svm.success = success;
            _svm.isLive = _config.GetValue<bool>("IsLive");

            return View(_svm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int studyId)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Study Details", "StudyID=" + studyId.ToString() + ", StudyID=" + studyId.ToString(), _ip.GetIPAddress());

            _svm.study = await _studyData.GetStudyDetails(studyId);
            _svm.studyList = await _studyData.GetStudyList();
            _svm.studyStatusList = await _studyData.GetStudyStatusList(); //because "recruitment status" is apparently "study status" for some reason

            return View(_svm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int studyId, string recruitedStatus, string studyStatus, DateTime? dateRecruited)
        {
            _svm.study = await _studyData.GetStudyDetails(studyId);
            string message = "";
            bool isSuccess = false;

            int success = await _crud.CallStoredProcedure("Study", "Edit", studyId,0,0, recruitedStatus, studyStatus, "", "", User.Identity.Name, dateRecruited, null, false, false);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Study-edit(SQL)" }); }

            message = "Study details updated.";
            isSuccess = true;

            return RedirectToAction("Index", "Study", new { mpi = _svm.study.MPI, message=message, success= isSuccess });
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int mpi)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Study Add", "MPI=" + mpi.ToString(), _ip.GetIPAddress());
            _svm.studyList = await _studyData.GetStudyList();
            _svm.studyStatusList = await _studyData.GetStudyStatusList();
            _svm.patient = await _patientData.GetPatientDetails(mpi);

            return View(_svm);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, string studyCode, string recruitedStatus, string studyStatus, DateTime? dateIdentified, DateTime? dateRecruited)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            string message = "";
            bool isSuccess = false;
            _svm.patient = await _patientData.GetPatientDetails(mpi);

            int success = await _crud.CallStoredProcedure("Study", "Create", mpi, 0, 0, studyCode, recruitedStatus, studyStatus, "", User.Identity.Name, dateIdentified, dateRecruited, false, false);
            
            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Study-new(SQL)" }); }

            message = "New study added.";
            isSuccess = true;

            return RedirectToAction("Index", "Study", new { mpi = _svm.patient.MPI, message = message, success = isSuccess });
        }
    }
}
