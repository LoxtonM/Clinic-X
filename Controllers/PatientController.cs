using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using APIControllers.Controllers;
using APIControllers.Data;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly APIContext _apiContext;
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
        private readonly APIController _api;

        public PatientController(ClinicalContext context, DocumentContext docContext, APIContext apiContext, IConfiguration config)
        {
            _clinContext = context;
            _docContext = docContext;
            _apiContext = apiContext;
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
            _constantsData = new ConstantsData(_docContext);
            _ageCalculator = new AgeCalculator();            
            _api = new APIController(_apiContext, _config);
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
                
                List<Patient> patients = new List<Patient>();
                patients = _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO);

                if (patients.Count > 0)
                {
                    int regNo;
                    string cguno = _pvm.patient.CGU_No;

                    if (Int32.TryParse(cguno.Substring(cguno.LastIndexOf('.') + 1), out regNo))
                    {
                        int prevRegNo = regNo - 1;
                        int nextRegNo = regNo + 1;

                        _pvm.previousPatient = _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + prevRegNo.ToString());
                        _pvm.nextPatient = _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + nextRegNo.ToString());
                    }
                    
                }

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

                if (_api.GetPhenotipsPatientID(id).Result != "")
                {                    
                    _pvm.isPatientInPhenotips = true;
                    _pvm.isCancerPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "Cancer").Result;
                    _pvm.isGeneralPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "General").Result;
                }

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
