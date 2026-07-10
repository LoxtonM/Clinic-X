using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIControllers.Controllers;

namespace ClinicX.Controllers
{
    public class LetterMenuController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ClinicalContext _context;
        private readonly DocumentContext _documentContext;
        private readonly IDocumentsDataAsync _documentsData;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;
        private readonly LettersMenuVM _lvm;
        private readonly LetterController _lc;
        private readonly ICRUD _crud;
        private readonly IDiaryDataAsync _diaryData;
        private readonly ILeafletDataAsync _leafletData;
        private readonly IExternalClinicianDataAsync _externalClinicianData;
        private readonly ITriageDataAsync _triageData;
        private readonly IScreeningCoordinatorDataAsync _screeningCoordinatorData;
        private readonly IApiController _api;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IRelativeDiaryDataAsync _relativeDiaryData;

        public LetterMenuController(IConfiguration config, IDocumentsDataAsync documentsData, IPatientDataAsync patientData, IReferralDataAsync referralData, LetterController letterController, ICRUD crud,
            IDiaryDataAsync diaryData, ILeafletDataAsync leafletData, IExternalClinicianDataAsync externalClinicianData, ITriageDataAsync triageData, 
            IScreeningCoordinatorDataAsync screeningCoordinatorData, IApiController api, IConstantsDataAsync constantsData, IRelativeDataAsync relativeData, IRelativeDiaryDataAsync relativeDiaryData,
            ClinicalContext context, DocumentContext documentContext) 
        {
            _config = config;   
            _context = context;
            _documentContext = documentContext;
            _documentsData = documentsData;
            _patientData = patientData;
            _referralData = referralData;
            _lvm = new LettersMenuVM();
            _lc = letterController;
            _crud = crud;
            _diaryData = diaryData;
            _leafletData = leafletData;
            _externalClinicianData = externalClinicianData;
            _triageData = triageData;
            _screeningCoordinatorData = screeningCoordinatorData;
            _relativeData = relativeData;
            _relativeDiaryData = relativeDiaryData;
            _api = api;
            _constantsData = constantsData;
        }

        [Authorize]
        public async Task<IActionResult> Index(int? refID, int mpi, int? relID = 0)
        {
            if (refID != null)
            {
                _lvm.referral = await _referralData.GetReferralDetails(refID.GetValueOrDefault());
            }

            _lvm.referralList = await _referralData.GetActiveReferralsListForPatient(mpi);

            if(refID == null && _lvm.referralList.Count() == 0)
            {
                return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = false, message = "No active referrals exist." });
            }

            var docList = await _documentsData.GetDocumentsList();
            _lvm.docsListStandard = docList.Where(d => d.DocGroup == "Standard").ToList(); //might need a "ListDocGroups" data model
            _lvm.docsListMedRec = docList.Where(d => d.DocGroup == "MEDREC").ToList();
            _lvm.docsListDNA = docList.Where(d => d.DocGroup == "DNATS").ToList();
            _lvm.docsListOutcome = docList.Where(d => d.DocGroup == "Outcome").ToList();
            _lvm.docsListReports = docList.Where(d => d.DocGroup == "REPORTS").ToList();
            _lvm.docsListOther = docList.Where(d => d.DocGroup == "OTHER").ToList();
            _lvm.docContentList = await _documentsData.GetDocumentsContentList();
            //_lvm.clinicianList = new List<ExternalCliniciansAndFacilities>();
            var clinList = await _externalClinicianData.GetClinicianList();
            var clins = new List<ExternalCliniciansAndFacilities>();
            clins = clinList;

            if (mpi == 0)
            {
                mpi = _lvm.referral.MPI;
            }

            _lvm.referralList = await _referralData.GetActiveReferralsListForPatient(mpi);
            _lvm.icpCancerList = await _triageData.GetCancerICPListForPatient(mpi);
            _lvm.patient = await _patientData.GetPatientDetails(mpi);
            _lvm.patGP = await _externalClinicianData.GetPatientGPReferrer(mpi);
            clins.Add(_lvm.patGP);
            clins = clins.Distinct().OrderBy(c => c.FACILITY).ToList();

            _lvm.externalClinicians = clins;            
            _lvm.histoList = clinList.Where(c => c.POSITION.Contains("Histo") || c.SPECIALITY.Contains("Histo")).OrderBy(c => c.FACILITY).ToList();
            _lvm.breastList = clinList.Where(c => c.POSITION.Contains("Breast") || c.SPECIALITY.Contains("Breast")).OrderBy(c => c.FACILITY).ToList();
            _lvm.geneticsList = clinList.Where(c => c.POSITION.Contains("Genetics") || c.SPECIALITY.Contains("Genetics")).OrderBy(c => c.FACILITY).ToList();
            
            
            _lvm.screeningCoordinatorList = await _screeningCoordinatorData.GetScreeningCoordinatorList();

            _lvm.leaflets = new List<Leaflet>();
            
            if (refID != null)
            {
                if (_lvm.referral.PATHWAY.Trim() == "Cancer") //because obviously it's not "cancer" it's "cancer space".
                {
                    _lvm.leaflets = await _leafletData.GetCancerLeafletsList();
                }
                else if (_lvm.referral.PATHWAY.Trim() == "General")
                {
                    _lvm.leaflets = await _leafletData.GetGeneralLeafletsList();
                }
            }
            else
            {
                _lvm.leaflets = await _leafletData.GetAllLeafletsList(); //in case there is no referral pathway listed
            }

            _lvm.relativeList = await _relativeData.GetRelativesList(mpi);

            _lvm.selectedRelativeID = relID.GetValueOrDefault();

            return View(_lvm);
        }

        [HttpPost]
        public async Task<IActionResult> DoLetter(int refID, string docCode, bool isPreview, string? additionalText, string? enclosures, string? otherClinician, 
            string? siteText, int? leafletID = 0, int? relID = 0)
        {
            var doc = await _documentsData.GetDocumentDetailsByDocCode(docCode);
            int docID = 0;

            if (docCode != "REPSUM")
            {
                docID = doc.DocContentID;
            }
            _lvm.referral = await _referralData.GetReferralDetails(refID);
            _lvm.patient = await _patientData.GetPatientDetails(_lvm.referral.MPI);

            int mpi = _lvm.patient.MPI;
            int diaryID = 0;


            if (!isPreview) //don't create a diary entry for every time we preview the letter!!
            {
                if (relID == 0)
                {
                    int success = await _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DoLetter-diary insert(SQL)" }); }

                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);

                    diaryID = diary.DiaryID; //get the diary ID of the entry just created to add to the letter's filename
                }
                else 
                {
                    int success = await _crud.CallStoredProcedure("RelativeDiary", "Create", relID.GetValueOrDefault(), 0, 0, "", "L", docCode, "", User.Identity.Name, DateTime.Now, null, false, false);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DoLetter-diary insert(SQL)" }); }

                    var diary = await _relativeDiaryData.GetLatestDiaryByDocCode(relID.GetValueOrDefault(), docCode);

                    diaryID = diary.DiaryID;
                }
            }

            LetterControllerLOCAL lc = new LetterControllerLOCAL(_context, _documentContext); //to test

            if (docCode == "REPSUM")
            {
                var icp = await _triageData.GetICPDetailsByRefID(refID);
                var icpC = await _triageData.GetCancerICPDetailsByICPID(icp.ICPID);

                return RedirectToAction("PrepareRepsum", "Repsum", new { id = icpC.ICP_Cancer_ID, diaryID = diaryID });
            }
            else
            {
                string qrCodeText = ""; //check and set up the Phenotips PPQ QR code if required
                bool isPhenotipsAvailable = await _constantsData.GetConstant("PhenotipsURL", 2) == "1";

                if (isPhenotipsAvailable)
                {
                    bool isPhenotipsQR = _documentsData.GetDocumentDetailsByDocCode(docCode).Result.hasPhenotipsPPQ;

                    if (isPhenotipsQR)
                    {
                        qrCodeText = await _api.GetPPQQRCode(mpi, _lvm.referral.PATHWAY);
                    }
                }

                await lc.DoPDF(docID, mpi, refID, User.Identity.Name, _lvm.referral.ReferrerCode, additionalText, enclosures, 0, "", false, false, diaryID, "", "", 0, otherClinician,
                        siteText, null, isPreview, qrCodeText, leafletID, true);
            }

            if(isPreview)
            {
                return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }

            return RedirectToAction("Index", new { refID = refID, mpi = mpi });
        }
    }
}
