using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.Data;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cXContext;
        private readonly DocumentContext _docContext;
        private readonly PatientVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;        
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly IPathwayData _pathwayData;
        private readonly IAlertData _alertData;
        private readonly IReferralData _referralData;
        private readonly IDiaryData _diaryData;
        private readonly IHPOCodeData _hpoData;
        private readonly IAuditService _audit;
        private readonly IConstantsData _constantsData;
        private readonly IAgeCalculator _ageCalculator;

        public PatientController(ClinicalContext context, ClinicXContext cXContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = context;
            _cXContext = cXContext;
            _docContext = docContext;
            _config = config;
            _pvm = new PatientVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _alertData = new AlertData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _hpoData = new HPOCodeData(_cXContext);
            _audit = new AuditService(_config);
            _constantsData = new ConstantsData(_docContext);
            _ageCalculator = new AgeCalculator();
        }
        

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id, bool? success, string? message)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;                
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);                
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Patient", "MPI=" + id.ToString(), _ip.GetIPAddress());

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

                if (_pvm.patient.DOB != null)
                {                    
                    if (_pvm.patient.DECEASED != 0)
                    {
                        _pvm.currentAge = _ageCalculator.DateDifferenceYear(_pvm.patient.DOB.GetValueOrDefault(), _pvm.patient.DECEASED_DATE.GetValueOrDefault());
                    }
                    else
                    {
                        _pvm.currentAge = _ageCalculator.DateDifferenceYear(_pvm.patient.DOB.GetValueOrDefault(), DateTime.Today);
                    }
                }

                if (!_constantsData.GetConstant("PhenotipsURL", 2).Contains("0"))
                {
                    _pvm.isPhenotipsAvailable = true;
                }

                if (success.HasValue)
                {
                    _pvm.ptSuccess = success.GetValueOrDefault();
                                        
                    if (message != null)
                    {
                        _pvm.message = message;
                    }
                }

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }
        
        
    }
}
