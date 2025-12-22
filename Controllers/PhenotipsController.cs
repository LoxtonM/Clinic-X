using APIControllers.Controllers;
//using APIControllers.Data;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;


namespace ClinicX.Controllers
{
    public class PhenotipsController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _docContext;
        //private readonly APIContext _apiContext;
        private readonly IConfiguration _config;
        private readonly APIController _api;
        private readonly IPatientData _patientData;
        private readonly IReferralData _referralData;
        private readonly PhenotipsVM _pvm;
        private readonly LetterController _lc;

        public PhenotipsController(IConfiguration config, APIController aPIController, IPatientData patientData, IReferralData referralData, LetterController lc)
        {
            //_clinContext = clinContext;
            //_docContext = docContext;
            //_apiContext = apiContext;
            _config = config;
            //_api = new APIController(_apiContext, _config);
            _api = aPIController;
            //_patientData = new PatientData(_clinContext);
            _patientData = patientData;
            _referralData = referralData;
            _pvm = new PhenotipsVM();
            _lc = lc;
        }

        [Authorize]
        public async Task<IActionResult> PushPatientToPt(int mpi)
        {
            string sMessage = "";
            bool isSuccess = false;

            Int16 result = await _api.PushPtToPhenotips(mpi); //initiates the push, returns 1 (success), 0 (already exists), or -1 (failed)

            if(result==1)
            {                
                string ptID = await _api.GetPhenotipsPatientID(mpi);
                Patient patient = _patientData.GetPatientDetails(mpi);
                AddPatientToPhenotipsMirrorTable(ptID, mpi, patient.CGU_No, patient.FIRSTNAME, patient.LASTNAME, patient.DOB.GetValueOrDefault(), patient.POSTCODE, patient.SOCIAL_SECURITY);
                isSuccess = true;
                sMessage = "Push to Phenotips successful";
            }
            else if (result == 0)
            {
                sMessage = "Patient already exists in Phenotips!";
            }
            else
            {
                sMessage = "Push to Phenotips failed :(";
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
        }

        void AddPatientToPhenotipsMirrorTable(string ptID, int mpi, string cguno, string firstname, string lastname, DateTime DOB, string postCode, string nhsNo)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Insert into dbo.PhenotipsPatients (PhenotipsID, MPI, CGUNumber, FirstName, Lastname, DOB, PostCode, NHSNo) values('"
                + ptID + "', " + mpi + ", '" + cguno + "', '" + firstname + "', '" + lastname + "', '" + DOB.ToString("yyyy-MM-dd") + "', '" + postCode +
                "', '" + nhsNo + "')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        [Authorize]
        public async Task<IActionResult> CreatePPQ(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;            

            Int16 result = await _api.SchedulePPQ(mpi, pathway); //creates the PPQ, returns 1 (success), 0 (already exists), or -1 (failed)

            if (result == 1)
            {
                isSuccess = true;
                sMessage = $"{pathway} PPQ created successfully";
            }
            else if (result == 0)
            {
                sMessage = $"{pathway} PPQ is already scheduled";
            }
            else
            {
                sMessage = "PPQ creation failed :(";
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
        }

        [Authorize]
        public String GetPPQURL(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;

            string result = _api.GetPPQUrl(mpi, pathway).Result; //fetches the URL

            return result.ToString();
        }

        [Authorize]
        public IActionResult PhenotipsQRLink(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;
            
            string result = _api.GetPPQQRCode(mpi, pathway).Result;
            
            _pvm.ppqQR = result.ToString(); // fetches the QR code string

            return View(_pvm);
        }

        [Authorize]
        public IActionResult SendPhenotipsLetter(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;
            string user = User.Identity.Name;
            string result = _api.GetPPQQRCode(mpi, pathway).Result;

            _pvm.ppqQR = result.ToString(); //fetches the QR code string

            //LetterController let = new LetterController(_clinContext, _docContext);
            
            //ReferralData refer = new ReferralData(_clinContext);
            Referral referral = _referralData.GetActiveReferralsListForPatient(mpi).OrderByDescending(r => r.RefDate).FirstOrDefault();

            if (referral != null)
            {
                if (pathway == "Cancer") //sends either ClicsFHF or Kc letter
                {
                    _lc.DoPDF(156, mpi, referral.refid, User.Identity.Name, referral.ReferrerCode, "", "", 0, "", false, false, 0, "", "", 0, "", "", null, true, result);
                }
                else
                {
                    _lc.DoPDF(191, mpi, referral.refid, User.Identity.Name, referral.ReferrerCode, "", "", 0, "", false, false, 0, "", "", 0, "", "", null, true, result);
                }

                System.IO.File.Delete($"wwwroot\\Images\\qrCode-{user}.jpg");

                return File($"~/StandardLetterPreviews/preview-{user}.pdf", "Application/PDF");
            }
            else
            {
                isSuccess = false;
                sMessage = "No active referrals found.";
                return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
            }
        }

        [Authorize]
        public IActionResult CreatePhenotipsEmail(int mpi, string pathway, bool isEmail)
        {
            Patient patient = _patientData.GetPatientDetails(mpi);

            string ppqURL = _api.GetPPQUrl(mpi, pathway).Result; //fetches the URL

            if (isEmail)
            {
                if (patient.EmailAddress == null)
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, message = "No email address recorded.", success = false });
                }
                if (!patient.EmailAddress.Contains("@"))
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, message = "Not a valid email address.", success = false });
                }

                return Redirect("mailto:" + patient.EmailAddress + "?subject=Phenotips PPQ&body=Here is your Phenotips PPQ link:%0D%0A%0D%0A" + ppqURL + "%0D%0A%0D%0APlease complete this asap.");
            }
            else
            {
                return RedirectToAction("PhenotipsPPQLink", "Phenotips", new { mpi=patient.MPI, ppqLink = ppqURL });
            }
        }

        [Authorize]
        public IActionResult PhenotipsPPQLink(int mpi, string ppqLink)
        {
            _pvm.mpi = mpi;
            _pvm.ppqURL = ppqLink;

            return View(_pvm); //returns the URL in a web page to copy/paste
        }        
    }
}
