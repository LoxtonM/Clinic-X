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
        private readonly ITriageData _triageData;
        private readonly APIController _api;
        private readonly PhenotipsMirrorData _phenotipsMirrorData;

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
            _triageData = new TriageData(_clinContext);
            _api = new APIController(_apiContext, _config);
            _phenotipsMirrorData = new PhenotipsMirrorData(_clinContext);
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
                
                List<Patient> patients = new List<Patient>(); //patients in the pedigree
                patients = _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO);

                if (patients.Count > 0) //to do the fwd and back buttons across the pedigree
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
                _pvm.referrals = _referralData.GetActiveReferralsListForPatient(id);
                _pvm.referrals = _pvm.referrals.Where(r => r.PATHWAY != null).ToList(); 
                //because there are nulls in the pathway that are breaking it!! So we have to filter them out.
                _pvm.referralsActiveGeneral = _pvm.referrals.Where(r => r.PATHWAY.Contains("General")).ToList();
                _pvm.referralsActiveCancer = _pvm.referrals.Where(r => r.PATHWAY.Contains("Cancer")).ToList();
                _pvm.patientPathway = _pathwayData.GetPathwayDetails(id);
                //_pvm.patientPathways = _pathwayData.GetPathways(id);
                _pvm.icpCancerList = new List<ICPCancer>();

                foreach (var r in _pvm.referralsActiveCancer)
                {   
                    ICP icp = _triageData.GetICPDetailsByRefID(r.refid);
                    ICPCancer icpc = _triageData.GetCancerICPDetailsByICPID(icp.ICPID);
                    _pvm.icpCancerList.Add(icpc);
                }

                _pvm.alerts = _alertData.GetAlertsList(id);
                _pvm.diary = _diaryData.GetDiaryList(id);

                if (!_constantsData.GetConstant("PhenotipsURL", 2).Contains("0") && _constantsData.GetConstant("PhenotipsURL", 2) != "")
                {                    
                    _pvm.isPhenotipsAvailable = true;
                }

                //Constants table flag decides whether Phenotips is in use or not
                if (_pvm.isPhenotipsAvailable) //pings the Phenotips API to see if a PPQ is scheduled
                {
                    //if (_api.GetPhenotipsPatientID(id).Result != "")
                    if (_phenotipsMirrorData.GetPhenotipsPatientByID(id) != null) //use the mirror table rather than pinging the API every time someone checks the record!
                    {
                        _pvm.isPatientInPhenotips = true;
                        _pvm.isCancerPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "Cancer").Result;
                        _pvm.isGeneralPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "General").Result;
                                                
                        _pvm.isCancerPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "Cancer").Result;
                        _pvm.isGeneralPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "General").Result;                        
                        _pvm.phenotipsLink = _constantsData.GetConstant("PhenotipsURL", 1) + "/" + _api.GetPhenotipsPatientID(id).Result;
                    }
                }

                if (_pvm.patient.DOB != null) //yes we do actually have null birth dates!
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

                

                if (success.HasValue)
                {
                    _pvm.ptSuccess = success.GetValueOrDefault();
                                        
                    if (message != null)
                    {
                        _pvm.message = message;
                    }
                }

                _pvm.edmsLink = _constantsData.GetConstant("GEMRLink", 1) + _pvm.patient.DCTM_Folder_ID + "/cg_view_pedigree_patie";

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }
        
        
    }
}
