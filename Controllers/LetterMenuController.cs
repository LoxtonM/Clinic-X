//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class LetterMenuController : Controller
    {
        private readonly IConfiguration _config;
        //private readonly ClinicalContext _context;
        //private readonly DocumentContext _documentContext;
        private readonly IDocumentsDataAsync _documentsData;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly LettersMenuVM _lvm;
        private readonly LetterController _lc;
        private readonly ICRUD _crud;
        private readonly IDiaryDataAsync _diaryData;
        private readonly ILeafletDataAsync _leafletData;
        private readonly IExternalClinicianDataAsync _externalClinicianData;

        public LetterMenuController(IConfiguration config, IDocumentsDataAsync documentsData, IPatientDataAsync patientData, IReferralDataAsync referralData, LetterController letterController, ICRUD crud,
            IDiaryDataAsync diaryData, ILeafletDataAsync leafletData, IExternalClinicianDataAsync externalClinicianData) 
        {
            _config = config;   
            //_context = context;
            //_documentContext = documentContext;
            _documentsData = documentsData;
            _patientData = patientData;
            _referralData = referralData;
            _lvm = new LettersMenuVM();
            _lc = letterController;
            _crud = crud;
            _diaryData = diaryData;
            _leafletData = leafletData;
            _externalClinicianData = externalClinicianData;
        }

        [Authorize]
        public async Task<IActionResult> Index(int refID)
        {
            _lvm.referral = await _referralData.GetReferralDetails(refID);
            _lvm.patient = await _patientData.GetPatientDetails(_lvm.referral.MPI);
            var docList = await _documentsData.GetDocumentsList();
            _lvm.docsListStandard = docList.Where(d => d.DocGroup == "Standard").ToList(); //might need a "ListDocGroups" data model
            _lvm.docsListMedRec = docList.Where(d => d.DocGroup == "MEDREC").ToList();
            _lvm.docsListDNA = docList.Where(d => d.DocGroup == "DNATS").ToList();
            _lvm.docsListOutcome = docList.Where(d => d.DocGroup == "Outcome").ToList();
            _lvm.externalClinicians = await _externalClinicianData.GetExternalCliniciansByType("histo");
            _lvm.leaflets = new List<Leaflet>();
            if (_lvm.referral.PATHWAY == "Cancer")
            {
                _lvm.leaflets = await _leafletData.GetCancerLeafletsList();
            }
            else if (_lvm.referral.PATHWAY == "General")
            {
                _lvm.leaflets = await _leafletData.GetGeneralLeafletsList();
            }
            else
            {
                _lvm.leaflets = await _leafletData.GetAllLeafletsList(); //in case there is no referral pathway listed
            }

            return View(_lvm);
        }

        [HttpPost]
        public async Task<IActionResult> DoLetter(int refID, string docCode, bool isPreview, string? additionalText, string? enclosures, string? otherClinician, int? leafletID = 0)
        {
            var doc = await _documentsData.GetDocumentDetailsByDocCode(docCode);
            int docID = doc.DocContentID;
            _lvm.referral = await _referralData.GetReferralDetails(refID);
            _lvm.patient = await _patientData.GetPatientDetails(_lvm.referral.MPI);

            int mpi = _lvm.patient.MPI;
            int diaryID = 0;


            if (!isPreview) //don't create a diary entry for every time we preview the letter!!
            {
                int success = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DoLetter-diary insert(SQL)" }); }

                var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                diaryID = diary.DiaryID; //get the diary ID of the entry just created to add to the letter's filename
            }

            //LetterControllerLOCAL lc = new LetterControllerLOCAL(_context, _documentContext); //to test

            _lc.DoPDF(docID, mpi, refID, User.Identity.Name, _lvm.referral.ReferrerCode, additionalText, enclosures, 0, "", false, false, diaryID, "", "", 0, otherClinician, 
                    "", null, isPreview, "", leafletID);

            if(isPreview)
            {
                return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }

            return RedirectToAction("Index", new { refID = refID });
        }
    }
}
