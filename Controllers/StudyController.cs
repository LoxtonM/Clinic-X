using ClinicalXPDataConnections.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class StudyController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IStudyDataAsync _studyData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IAuditService _audit;
        private readonly StudyVM _svm;

        public StudyController(IConfiguration config, IStudyDataAsync studyData, IStaffUserDataAsync staffUserData, IAuditService audit)
        {
            _config = config;
            _studyData = studyData;
            _svm = new StudyVM();
            _staffUser = staffUserData;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int mpi)
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Study List", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

            _svm.studies = await _studyData.GetStudiesForPatient(mpi);

            _svm.isLive = _config.GetValue<bool>("IsLive");

            return View(_svm);
        }
    }
}
