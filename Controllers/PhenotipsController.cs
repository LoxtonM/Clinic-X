using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;
using APIControllers.Controllers;
using APIControllers.Data;
using ClinicX.ViewModels;
using System.Security.Cryptography.Xml;


namespace ClinicX.Controllers
{
    public class PhenotipsController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly APIContext _apiContext;
        private readonly IConfiguration _config;
        private readonly APIController _api;
        private readonly IPatientData _patientData;
        private readonly PhenotipsVM _pvm;

        public PhenotipsController(ClinicalContext clinContext, DocumentContext docContext, APIContext apiContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _apiContext = apiContext;
            _config = config;
            _api = new APIController(_apiContext, _config);
            _patientData = new PatientData(_clinContext);
            _pvm = new PhenotipsVM();
        }

        public async Task<IActionResult> PushPatientToPt(int mpi)
        {
            string sMessage = "";
            bool isSuccess = false;

            Int16 result = await _api.PushPtToPhenotips(mpi);

            if(result==1)
            {
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

        public async Task<IActionResult> CreatePPQ(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;            

            Int16 result = await _api.SchedulePPQ(mpi, pathway);

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

        public async Task<String> GetPPQURL(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;

            string result = _api.GetPPQUrl(mpi, pathway).Result;

            return result.ToString();
        }

        public async Task<IActionResult> PhenotipsQRLink(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;
            
            string result = _api.GetPPQQRCode(mpi, pathway).Result;
            
            _pvm.ppqQR = result.ToString();

            return View(_pvm);
        }

        public async Task<IActionResult> SendPhenotipsLetter(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;
            string user = User.Identity.Name;
            string result = _api.GetPPQQRCode(mpi, pathway).Result;

            _pvm.ppqQR = result.ToString();

            LetterControllerLOCAL let = new LetterControllerLOCAL(_clinContext, _docContext);
            
            ReferralData refer = new ReferralData(_clinContext);
            Referral referral = refer.GetActiveReferralsListForPatient(mpi).OrderByDescending(r => r.RefDate).FirstOrDefault();

            if (referral != null)
            {

                if (pathway == "Cancer")
                {
                    let.DoPDF(156, mpi, referral.refid, User.Identity.Name, referral.ReferrerCode, "", "", 0, "", false, false, 0, "", "", 0, "", "", null, true, result);
                }
                else
                {
                    let.DoPDF(191, mpi, referral.refid, User.Identity.Name, referral.ReferrerCode, "", "", 0, "", false, false, 0, "", "", 0, "", "", null, true, result);
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


        public async Task<IActionResult> CreatePhenotipsEmail(int mpi, string pathway, bool isEmail)
        {
            Patient patient = _patientData.GetPatientDetails(mpi);

            string ppqURL = _api.GetPPQUrl(mpi, pathway).Result;

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

        public async Task<IActionResult> PhenotipsPPQLink(int mpi, string ppqLink)
        {
            _pvm.mpi = mpi;
            _pvm.ppqURL = ppqLink;

            return View(_pvm);
        }
        
    }
}
