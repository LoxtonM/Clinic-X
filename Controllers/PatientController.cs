using APIControllers.Controllers;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _docContext;
        //private readonly APIContext _apiContext;
        private readonly PatientVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;        
        private readonly IPatientDataAsync _patientData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IPathwayDataAsync _pathwayData;
        private readonly IAlertDataAsync _alertData;
        private readonly IReferralDataAsync _referralData;
        private readonly IDiaryDataAsync _diaryData;
        private readonly IHPOCodeDataAsync _hpoData;
        private readonly IAuditService _audit;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IAgeCalculator _ageCalculator;
        private readonly ITriageDataAsync _triageData;
        private readonly IClinicDataAsync _clinicData;
        private readonly IApiController _api;
        private readonly IPhenotipsMirrorDataAsync _phenotipsMirrorData;

        public PatientController(IConfiguration config, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData, IRelativeDataAsync relativeData, IPathwayDataAsync pathwayData, IAlertDataAsync alertData, 
            IReferralDataAsync referralData, IDiaryDataAsync diaryData, IHPOCodeDataAsync hPOCodeData, IAuditService auditService, IConstantsDataAsync constantsData, IAgeCalculator ageCalculator,
            ITriageDataAsync triageData, IClinicDataAsync clinicData, IApiController aPIController, IPhenotipsMirrorDataAsync phenotipsMirrorData)
        {
            //_clinContext = context;
            //_docContext = docContext;
            //_apiContext = apiContext;
            _config = config;
            _pvm = new PatientVM();
            _staffUser = staffUserData;
            _patientData = patientData;
            _relativeData = relativeData;
            _pathwayData = pathwayData;
            _alertData = alertData;
            _referralData = referralData;
            _diaryData = diaryData;
            _hpoData = hPOCodeData;
            _audit = auditService;
            _constantsData = constantsData;
            _ageCalculator = ageCalculator;
            _triageData = triageData;
            _clinicData = clinicData;
            _api = aPIController;
            _phenotipsMirrorData = phenotipsMirrorData;
        }
        

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id, bool? success, string? message)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember?.STAFF_CODE ?? string.Empty;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);                
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Patient", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _pvm.patient = await _patientData.GetPatientDetails(id);

                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                var patientsInPedigree = await _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO);

                _pvm.relatives = await _relativeData.GetRelativesList(id);
                _pvm.hpoTermDetails = await _hpoData.GetHPOTermsAddedList(id);
                _pvm.referrals = await _referralData.GetActiveReferralsListForPatient(id);
                _pvm.appointmentList = await _clinicData.GetClinicByPatientsList(_pvm.patient.MPI); //.GroupBy(a => a.RefID).Select(g => g.First()).ToList();                
                _pvm.patientPathway = await _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = await _alertData.GetAlertsList(id);
                _pvm.diary = await _diaryData.GetDiaryList(id);

                _pvm.relatives = _pvm.relatives.Distinct().ToList(); //because there are dupes.
                _pvm.appointmentList = _pvm.appointmentList.Distinct().ToList();
                _pvm.icpCancerList = new List<ICPCancer>();                                

                if (patientsInPedigree.Count > 1 && !string.IsNullOrWhiteSpace(_pvm.patient.CGU_No)) //to do the fwd and back buttons across the pedigree
                {                    
                    string cguno = _pvm.patient.CGU_No;
                    var pointNo = cguno.LastIndexOf('.');

                    if (Int32.TryParse(cguno.Substring(pointNo + 1), out int regNo))
                    {
                        int prevRegNo = regNo - 1;
                        int nextRegNo = regNo + 1;

                        _pvm.previousPatient = await _patientData.GetPatientDetailsByCGUNo($"{_pvm.patient.PEDNO}.{prevRegNo}");
                        _pvm.nextPatient = await _patientData.GetPatientDetailsByCGUNo($"{_pvm.patient.PEDNO}.{nextRegNo}");
                    }                    
                }                

                IEnumerable<Referral> referrals = _pvm.referrals.Where(r => !string.IsNullOrWhiteSpace(r.PATHWAY));
                Console.WriteLine("Referrals without null pw in  " +  sw.ElapsedMilliseconds);
                //because there are nulls in the pathway that are breaking it!! So we have to filter them out.
                _pvm.referralsActiveGeneral = referrals.Where(r => r.PATHWAY.Contains("General", StringComparison.OrdinalIgnoreCase)).ToList();
                _pvm.referralsActiveCancer = referrals.Where(r => r.PATHWAY.Contains("Cancer", StringComparison.OrdinalIgnoreCase)).ToList();

                List<ICPCancer> icpCancerList = new List<ICPCancer>();

                foreach (var r in _pvm.referralsActiveCancer)
                {   
                    ICP icp = await _triageData.GetICPDetailsByRefID(r.refid);
                    if (icp != null)
                    {
                        ICPCancer icpc = await _triageData.GetCancerICPDetailsByICPID(icp.ICPID);
                        if (icpc != null) { _pvm.icpCancerList.Add(icpc); }
                    }
                }

                var phenotipsAvailableFlag = await _constantsData.GetConstant("PhenotipsURL", 2);
                _pvm.isPhenotipsAvailable = !string.IsNullOrEmpty(phenotipsAvailableFlag) && !phenotipsAvailableFlag.Contains("0", StringComparison.Ordinal);

                //Constants table flag decides whether Phenotips is in use or not
                if (_pvm.isPhenotipsAvailable)
                {
                    var mirror = _phenotipsMirrorData.GetPhenotipsPatientByID(id);

                    if (mirror != null) //use the mirror table rather than pinging the API every time someone checks the record!
                    {
                        _pvm.isPatientInPhenotips = true;
                        
                        var cancerPPQExists = _api.CheckPPQExists(_pvm.patient.MPI, "Cancer");
                        var generalPPQExists = _api.CheckPPQExists(_pvm.patient.MPI, "General");
                        var cancerPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "Cancer");
                        var generalPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "General");
                        var phenotipsID = _api.GetPhenotipsPatientID(id);

                        await Task.WhenAll(cancerPPQExists, generalPPQExists, cancerPPQComplete, generalPPQComplete, phenotipsID);

                        //Task.WaitAll(cancerPPQExists, generalPPQExists, cancerPPQComplete, generalPPQComplete, phenotipsID);

                        _pvm.isCancerPPQScheduled = await cancerPPQExists;
                        _pvm.isGeneralPPQScheduled = await generalPPQExists;
                        _pvm.isCancerPPQComplete = await cancerPPQComplete;
                        _pvm.isGeneralPPQComplete = await generalPPQComplete;
                        string ptID = await phenotipsID;

                        string baseURL = await _constantsData.GetConstant("PhenotipsURL", 1);

                        if(!string.IsNullOrWhiteSpace(baseURL) && !string.IsNullOrWhiteSpace(ptID)) { _pvm.phenotipsLink = baseURL.TrimEnd('/') + "/" + ptID; }
                    }
                }

                if (_pvm.patient.DOB.HasValue) //yes we do actually have null birth dates!
                {                    
                    var endDate = (_pvm.patient.DECEASED != 0 && _pvm.patient.DECEASED_DATE.HasValue) ? _pvm.patient.DECEASED_DATE.Value : DateTime.Today;

                    _pvm.currentAge = _ageCalculator.DateDifferenceYear(_pvm.patient.DOB.Value, endDate);

                }

                if (success.HasValue)
                {
                    _pvm.ptSuccess = success.Value;
                                        
                    if (!string.IsNullOrEmpty(message)) { _pvm.message = message; }
                }

                //EDMS link
                string edmsBaseURL = await _constantsData.GetConstant("GEMRLink", 1);

                if (!string.IsNullOrWhiteSpace(edmsBaseURL) && !string.IsNullOrWhiteSpace(_pvm.patient.DCTM_Folder_ID))
                {
                    _pvm.edmsLink = edmsBaseURL + _pvm.patient.DCTM_Folder_ID + "/cg_view_pedigree_patie";
                }

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpGet]
        public IActionResult PedigreeAssistant(int id)
        {
            RunPedigreeAssistant();

            return RedirectToAction("PatientDetails", new { id = id });
        }

        public void RunPedigreeAssistant()
        {
            //Process.Start($"G:\\WMFACS databases\\Pedigree drawing\\GeneticPedigree.exe");
            Process.Start($"\\\\zion.matrix.local\\dfsrootbwh\\cling\\WMFACS databases\\Pedigree drawing\\GeneticPedigree.exe");
        }
    }
}