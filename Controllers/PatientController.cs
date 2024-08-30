using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;
using RestSharp;

namespace ClinicX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
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
        private readonly IConstantsData _constants;

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
            _constants = new ConstantsData(_clinContext);
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
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }
        
        public async Task<IActionResult> PushPtToPhenotips(int id)
        {
            _pvm.patient = _patientData.GetPatientDetails(id);
            string firstName = _pvm.patient.FIRSTNAME;
            string lastName = _pvm.patient.LASTNAME;
            DateTime DOB = _pvm.patient.DOB.GetValueOrDefault();
            int yob = DOB.Year;
            int mob = DOB.Month;
            int dob = DOB.Day;
            string cguNo = _pvm.patient.CGU_No;
            string gender = _pvm.patient.SEX.Substring(0, 1);
            string apiURL = _constants.GetConstant("PhenotipsURL", 1).Trim(); //because for some reason, the Constants table padds all fields with lots of white space!!
            apiURL = apiURL + ":443/rest/patients";            
            var options = new RestClientOptions(apiURL);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("authorization", "Basic UGV0ZXJOZXdjb21iZTpiS1d6ZW5tNkFEM0VYZHNyQ0ZCY000aFY=");
            //string apiCall = "{\"patient_name\":{\"first_name\":\"firstName\",\"last_name\":\"lastName\"},\"date_of_birth\":{\"year\":9999,\"month\":8888,\"day\":7777},\"sex\":\"gender\",\"external_id\":\"cguNo\"}";
            string apiCall = "{\"patient_name\":{\"first_name\":\"" + $"{firstName}" + "\",\"last_name\":\"" + $"{lastName}";
            apiCall = apiCall + "\"},\"date_of_birth\":{\"year\":" + yob.ToString() + ",\"month\":" + mob.ToString() + ",\"day\":" + dob.ToString();
            apiCall = apiCall + "},\"sex\":\"" + $"{gender}" + "\",\"external_id\":\"" + $"{cguNo}" + "\"}";
            
            request.AddJsonBody(apiCall, false);
            var response = await client.PostAsync(request);

            return RedirectToAction("PatientDetails", "Patient", new { id = _pvm.patient.MPI });
        }
    }
}
