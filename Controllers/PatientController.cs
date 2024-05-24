using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly PatientVM _pvm;
        private readonly StaffUserData _staffUser;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly IPathwayData _pathwayData;
        private readonly IAlertData _alertData;
        private readonly IReferralData _referralData;
        private readonly IDiaryData _diaryData;
        private readonly IHPOCodeData _hpoData;
        private readonly IAuditService _audit;

        public PatientController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _pvm = new PatientVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _alertData = new AlertData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _hpoData = new HPOCodeData(_clinContext);
            _audit = new AuditService(_config);
        }
        

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Patient", "MPI=" + id.ToString());

                _pvm.patient = _patientData.GetPatientDetails(id);
                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                _pvm.relatives = _relativeData.GetRelativesList(id).Distinct().ToList();
                _pvm.hpoTermDetails = _hpoData.GetHPOTermsAddedList(id);
                _pvm.referrals = _referralData.GetReferralsList(id);
                _pvm.patientPathway = _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = _alertData.GetAlertsList(id);
                _pvm.diary = _diaryData.GetDiaryList(id);

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        
    }
}
