using ClinicalXPDataConnections.Data;
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
        private readonly IDocumentsData _documentsData;
        private readonly IPatientData _patientData;
        private readonly IReferralData _referralData;
        private readonly LettersMenuVM _lvm;
        private readonly LetterController _lc;
        private readonly ICRUD _crud;
        private readonly IDiaryData _diaryData;
        private readonly ILeafletData _leafletData;
        private readonly IExternalClinicianData _externalClinicianData;

        public LetterMenuController(IConfiguration config, IDocumentsData documentsData, IPatientData patientData, IReferralData referralData, LetterController letterController, ICRUD crud,
            IDiaryData diaryData, ILeafletData leafletData, IExternalClinicianData externalClinicianData) 
        {
            _config = config;   
            //_context = context;
            //_documentContext = documentContext;
            //_documentsData = new DocumentsData(_documentContext);
            _documentsData = documentsData;
            //_patientData = new PatientData(_context);
            _patientData = patientData;
            //_referralData = new ReferralData(_context);
            _referralData = referralData;
            _lvm = new LettersMenuVM();
            //_lc = new LetterController(_context, _documentContext);
            _lc = letterController;
            //_crud = new CRUD(_config);
            _crud = crud;
            //_diaryData = new DiaryData(_context);
            _diaryData = diaryData;
            //_leafletData = new LeafletData(_documentContext);
            _leafletData = leafletData;
            //_externalClinicianData = new ExternalClinicianData(_context);
            _externalClinicianData = externalClinicianData;
        }

        [Authorize]
        public IActionResult Index(int refID)
        {
            _lvm.referral = _referralData.GetReferralDetails(refID);
            _lvm.patient = _patientData.GetPatientDetails(_lvm.referral.MPI);
            _lvm.docsListStandard = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "Standard").ToList(); //might need a "ListDocGroups" data model
            _lvm.docsListMedRec = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "MEDREC").ToList();
            _lvm.docsListDNA = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "DNATS").ToList();
            _lvm.docsListOutcome = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "Outcome").ToList();
            _lvm.externalClinicians = _externalClinicianData.GetExternalCliniciansByType("histo");
            _lvm.leaflets = new List<Leaflet>();
            if (_lvm.referral.PATHWAY == "Cancer")
            {
                _lvm.leaflets = _leafletData.GetCancerLeafletsList();
            }
            else if (_lvm.referral.PATHWAY == "General")
            {
                _lvm.leaflets = _leafletData.GetGeneralLeafletsList();
            }
            else
            {
                _lvm.leaflets = _leafletData.GetAllLeafletsList(); //in case there is no referral pathway listed
            }

            return View(_lvm);
        }

        [HttpPost]
        public IActionResult DoLetter(int refID, string docCode, bool isPreview, string? additionalText, string? enclosures, string? otherClinician, int? leafletID = 0)
        {
            int docID = _documentsData.GetDocumentDetailsByDocCode(docCode).DocContentID;
            _lvm.referral = _referralData.GetReferralDetails(refID);
            _lvm.patient = _patientData.GetPatientDetails(_lvm.referral.MPI);

            int mpi = _lvm.patient.MPI;
            int diaryID = 0;


            if (!isPreview) //don't create a diary entry for every time we preview the letter!!
            {
                int success = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DoLetter-diary insert(SQL)" }); }

                diaryID = _diaryData.GetLatestDiaryByRefID(refID, docCode).DiaryID; //get the diary ID of the entry just created to add to the letter's filename
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
