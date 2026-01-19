//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
//using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;

namespace ClinicX.Controllers
{
    public class TriageController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        //private readonly DocumentContext _docContext;
        private readonly ICPVM _ivm;
        private readonly LetterController _lc;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPathwayDataAsync _pathwayData;
        private readonly IPriorityDataAsync _priorityData;
        private readonly IReferralDataAsync _referralData;
        private readonly ITriageDataAsync _triageData;
        private readonly IICPActionDataAsync _icpActionData;
        private readonly IRiskDataAsync _riskData;
        private readonly ISurveillanceDataAsync _survData;
        private readonly ITestEligibilityDataAsync _testEligibilityData;
        private readonly IDiaryDataAsync _diaryData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly ICancerRequestDataAsync _cancerRequestData;
        private readonly IExternalClinicianDataAsync _clinicianData;
        private readonly IRelativeDiagnosisDataAsync _relDiagData;
        private readonly IDocumentsDataAsync _documentsData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IAgeCalculator _ageCalculator;
        private readonly IPatientDataAsync _patientData;
        private readonly ILeafletDataAsync _leafletData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IStaffOptionsDataAsync _staffOptionsData;

        public TriageController(IConfiguration config, IStaffUserDataAsync staffUserData, IPathwayDataAsync pathwayData, IPriorityDataAsync priorityData, IReferralDataAsync referralData, ITriageDataAsync triageData,
            IICPActionDataAsync iCPActionData, IRiskDataAsync riskData, ISurveillanceDataAsync surveillanceData, ITestEligibilityDataAsync testEligibilityData, IDiaryDataAsync diaryData, IRelativeDataAsync relativeData,
            ICancerRequestDataAsync cancerRequestData, IExternalClinicianDataAsync externalClinicianData, IRelativeDiagnosisDataAsync relativeDiagnosisData, IDocumentsDataAsync documentsData, ICRUD crud, 
            LetterController letterController, IAuditService auditService, IAgeCalculator ageCalculator, IPatientDataAsync patientData, ILeafletDataAsync leafletData, IConstantsDataAsync constantsData,
            IStaffOptionsDataAsync staffOptionsData)
        {
            //_clinContext = clinContext;
            //_cXContext = cXContext;
            //_docContext = docContext;
            _config = config;
            _ivm = new ICPVM();
            _staffUser = staffUserData;
            _pathwayData = pathwayData;
            _priorityData = priorityData;
            _referralData = referralData;
            _triageData = triageData;
            _icpActionData = iCPActionData;
            _riskData = riskData;
            _survData = surveillanceData;
            _testEligibilityData = testEligibilityData;
            _diaryData = diaryData;
            _relativeData = relativeData;
            _cancerRequestData = cancerRequestData;
            _clinicianData = externalClinicianData;
            _relDiagData = relativeDiagnosisData;
            _documentsData = documentsData;
            _crud = crud;            
            _audit = auditService;
            _ageCalculator = ageCalculator;
            _patientData = patientData;
            _leafletData = leafletData;
            _constantsData = constantsData;
            _staffOptionsData = staffOptionsData;
            _lc = letterController;
            //LetterController _lc = new LetterController(_clinContext, _docContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                _ivm.staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(_ivm.staffCode, "ClinicX - Triage", "", _ip.GetIPAddress());

                var triages = await _triageData.GetTriageList(User.Identity.Name);
                _ivm.triages = triages.OrderBy(t => t.RefDate).ToList();
                var icpCancerList = await _triageData.GetCancerICPList(User.Identity.Name);
                _ivm.icpCancerListOwn = icpCancerList.Where(r => r.GC_CODE == _ivm.staffCode).ToList();
                _ivm.icpCancerListOther = icpCancerList.Where(r => r.ToBeReviewedby == User.Identity.Name.ToUpper() && r.FinalReviewed == null).ToList();
                
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage" });
            }
        }

        [Authorize]
        public async Task<IActionResult> ICPDetails(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - ICP Details", "ID=" + id.ToString(), _ip.GetIPAddress());

                _ivm.triage = await _triageData.GetTriageDetails(id);
                _ivm.patient = await _patientData.GetPatientDetails(_ivm.triage.MPI);

                int cancerICPCount = await _triageData.GetCancerICPCountByICPID(id);
                int generalICPCount = await _triageData.GetGeneralICPCountByICPID(id);

                if (cancerICPCount > 0 || generalICPCount > 0) { _ivm.isICPTriageStarted = true; }

                if (_ivm.triage == null) { return RedirectToAction("NotFound", "WIP"); }

                _ivm.referralDetails = await _referralData.GetReferralDetails(_ivm.triage.RefID);
                _ivm.clinicalFacilityList = await _triageData.GetClinicalFacilitiesList();
                _ivm.clinicalFacilityList = _ivm.clinicalFacilityList.OrderBy(f => f.NAME).ToList();
                _ivm.icpGeneral = await _triageData.GetGeneralICPDetailsByICPID(id);
                _ivm.icpCancer = await _triageData.GetCancerICPDetailsByICPID(id);
                _ivm.cancerActionsList = await _icpActionData.GetICPCancerActionsList();                
                _ivm.generalActionsList = await _icpActionData.GetICPGeneralActionsList();
                _ivm.generalActionsList2 = await _icpActionData.GetICPGeneralActionsList2();
                //check user type for cons/GC triage
                _ivm.loggedOnUserType = user.CLINIC_SCHEDULER_GROUPS;
                _ivm.priorityList = await _priorityData.GetPriorityList();
                _ivm.cancerReviewActionsLists = await _icpActionData.GetICPCancerReviewActionsList();
                _ivm.edmsLink = await _constantsData.GetConstant("GEMRLink", 1);
                _ivm.dobAt16 = DateTime.Now.AddYears(-16);
                _ivm.staffOptions = await _staffOptionsData.GetStaffOptions(staffCode);
                var clins = await _staffUser.GetClinicalStaffList();
                clins = clins.Where(s => s.POSITION.Contains("Genomic") && !s.POSITION.Contains("Nurse")).ToList(); //because sometimes they spell it without the s (and we have to filter out the hbopathy nurses too)
                _ivm.GAs = clins.Where(s => s.POSITION.Contains("Associate")).ToList();
                _ivm.GenPs = clins.Where(s => s.POSITION.Contains("Practitioner")).ToList();

                if (_ivm.referralDetails.RefDate != null)
                {
                    _ivm.referralAgeDays = _ageCalculator.DateDifferenceDay(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                    _ivm.referralAgeWeeks = _ageCalculator.DateDifferenceWeek(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                }

                if (_ivm.patient.DOB != null)
                {
                    _ivm.patientAge = _ageCalculator.DateDifferenceYear(_ivm.patient.DOB.GetValueOrDefault(), DateTime.Today);                    
                }
                _ivm.patientAddress = _ivm.patient.ADDRESS1; //build the address string - making sure to ignore the nulls
                if(_ivm.patient.ADDRESS2 != null) { _ivm.patientAddress = _ivm.patientAddress + ", " + _ivm.patient.ADDRESS2; }
                if (_ivm.patient.ADDRESS3 != null) { _ivm.patientAddress = _ivm.patientAddress + ", " + _ivm.patient.ADDRESS3; }
                if (_ivm.patient.ADDRESS4 != null) { _ivm.patientAddress = _ivm.patientAddress + ", " + _ivm.patient.ADDRESS4; }
                
                _ivm.patientAddress = _ivm.patientAddress + ", " + _ivm.patient.POSTCODE;

                if (DateTime.Now.AddYears(-16) < _ivm.patient.DOB) { _ivm.isChild = true; }

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-ICP" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int icpID, string? facility, int? duration, string? comment, bool isSPR, bool isChild, int? tp, int? tp2c, 
            int? tp2nc, int? wlPriority, int? requestPhotos, int? requestDevForm, string? ga, string? genp)
        {
            try
            {
                ICP icp = await _triageData.GetICPDetails(icpID);
                Referral referral = await _referralData.GetReferralDetails(icp.REFID);
                StaffMember staffmember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string wlClinician = "";
                
                int mpi = icp.MPI;
                int refID = icp.REFID;
                int tp2;
                string referrer = referral.ReferrerCode;
                string sApptIntent = "";
                string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;                            

                if (ga != null)
                {
                    wlClinician = ga;
                    facility = await _constantsData.GetConstant("GAClinic", 1);
                }
                if(genp != null) 
                { 
                    wlClinician = genp;
                    facility = await _constantsData.GetConstant("GenPClinic", 1);
                }

                if (comment == null) { comment = ""; }

                if (tp2c != null) { tp2 = tp2c.GetValueOrDefault(); }
                else { tp2 = tp2nc.GetValueOrDefault(); }

                if (tp2 == 3) { sApptIntent = "CLICS"; }

                if (sStaffType == "Consultant")
                {
                    if(wlClinician == "")
                    {
                        wlClinician = referral.PATIENT_TYPE_CODE;
                    }

                    if(tp.GetValueOrDefault() == 3 || tp.GetValueOrDefault() == 5)
                    {
                        tp2 = 0;
                    }

                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), tp2,
                        facility, sApptIntent, wlClinician, comment, User.Identity.Name, null, null, isSPR, isChild, duration, requestPhotos, requestDevForm);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage" }); }
                        
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), tp2,
                        "", sApptIntent, wlClinician, comment, User.Identity.Name, null, null, false, false, 0, requestPhotos, requestDevForm, ga, genp);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                    }
                }
                else
                {
                    if (wlClinician == "")
                    {
                        wlClinician = referral.GC_CODE;
                    }

                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), tp2,
                        facility, sApptIntent, wlClinician, comment, User.Identity.Name, null, null, isSPR, isChild, duration, requestPhotos, requestDevForm);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                        
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), tp2,
                        "", sApptIntent, wlClinician, comment, User.Identity.Name);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                    }
                }
                //add to waiting list
                if (facility != null && facility != "")
                {
                    int success = _crud.CallStoredProcedure("Waiting List", "Create", mpi, wlPriority.GetValueOrDefault(), referral.refid, facility, "General", wlClinician,
                        comment, User.Identity.Name);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genAddWL(SQL)" }); }
                }

                if (tp2 == 2) //CTB letter
                {
                    int success = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", "CTBAck", "", "", User.Identity.Name);
                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genDiaryUpdate(SQL)" }); }
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, "CTBAck");
                    int diaryID = diary.DiaryID;
                    
                    _lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID);
                }                

                if (tp2 == 6) //Dictate letter
                { 
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "", "", "", User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                }

                if (tp2 == 7) //Reject letter
                {                    
                    _lc.DoPDF(208, mpi, referral.refid, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-genTriage" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int icpID, int action)
        {
            try
            {
                ICP icp = await _triageData.GetICPDetails(icpID);
                int mpi = icp.MPI;
                int refID = icp.REFID;
                Referral referral = await _referralData.GetReferralDetails(refID);
                string referrer = referral.ReferrerCode;

                CRUD _crud = new CRUD(_config);
                int success = _crud.CallStoredProcedure("ICP Cancer", "Triage", icpID, action, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canTriage(SQL)" }); }

                var actions = await _icpActionData.GetICPCancerActionsList();
                ICPAction icpAction = actions.First(a => a.ID == action);
                
                if (icpAction.RelatedLetterID != null)
                {
                    var doc = await _documentsData.GetDocumentDetails(icpAction.RelatedLetterID.Value);
                    string docCode = doc.DocCode;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode); //to add diaryid to the letter output
                    int diaryID = diary.DiaryID;
                    
                    _lc.DoPDF(icpAction.RelatedLetterID.GetValueOrDefault(), mpi, refID, User.Identity.Name, referrer,"","",0,"",false,false,diaryID);
                }


                _ivm.icpCancer = await _triageData.GetCancerICPDetailsByICPID(icpID);

                return RedirectToAction("CancerReview", "Triage", new {id = _ivm.icpCancer.ICP_Cancer_ID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canTriage" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CancerReview(int id, string? message, bool? success)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP");}

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Cancer Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                URLChecker urlChecker = new URLChecker();
                string urlGenomicTestDirectoryurl = await _constantsData.GetConstant("TestDirectoryGeneral", 1); //apparently they want Rare Diseases, not the cancer one
                string urlCanriskurl = await _constantsData.GetConstant("CanriskURL", 1);

                _ivm.genomicsTestDirectoryLink = urlGenomicTestDirectoryurl;
                _ivm.canriskLink = urlCanriskurl;

                _ivm.clinicalFacilityList = await _triageData.GetClinicalFacilitiesList();
                _ivm.staffMembers = await _staffUser.GetClinicalStaffList();
                _ivm.icpCancer = await _triageData.GetCancerICPDetails(id);
                _ivm.leaflets = await _leafletData.GetCancerLeafletsList();

                if (_ivm.icpCancer != null)
                {
                    _ivm.riskList = await _riskData.GetRiskList(id);
                    _ivm.surveillanceList = await _survData.GetSurveillanceList(_ivm.icpCancer.MPI);
                    _ivm.eligibilityList = await _testEligibilityData.GetTestingEligibilityList(_ivm.icpCancer.MPI);
                }
                _ivm.documentList = await _documentsData.GetDocumentsList();
                _ivm.documentList = _ivm.documentList.Where(d => (d.DocCode.StartsWith("O") && d.DocGroup == "Outcome") || d.DocCode.Contains("PrC")).ToList();
                _ivm.cancerReviewActionsLists = await _icpActionData.GetICPCancerReviewActionsList();

                _ivm.message = message;
                _ivm.success = success.GetValueOrDefault();

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canReview" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancerReview(int id, string finalReview, string? clinician = "", string? clinic = "", string? comments = "", 
            string? addNotes = "", bool? isNotForCrossBooking = false, int? letter = 0, string? toBeReviewedBy = "", string? freeText1="", int? leafletID = 0) //, 
            
        {
            var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;

            _ivm.icpCancer = await _triageData.GetCancerICPDetails(id);
            var icpDetails = await _triageData.GetICPDetails(_ivm.icpCancer.ICPID);
            string reviewText = "";
            
            string reviewBy = "";
            _ivm.cancerReviewActionsLists = await _icpActionData.GetICPCancerReviewActionsList();
            
            int mpi = icpDetails.MPI;
            int refID = icpDetails.REFID;

            if (letter != null && letter != 0)
            {                
                _ivm.cancerAction = await _icpActionData.GetICPCancerAction(letter.GetValueOrDefault());
                string docCode = _ivm.cancerAction.DocCode;

                if (letter != 1 && letter != 11)
                {
                    reviewText = docCode;

                    if (reviewText != null) { reviewText = reviewText + " letter on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + user.NAME; }

                    string diaryText = "";
                    var doc = await _documentsData.GetDocumentDetailsByDocCode(docCode);
                    int letterID = doc.DocContentID;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", diaryText, User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    int diaryID = diary.DiaryID;

                    if (letter == 3)
                    {
                        int successDOT = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "", staffCode, "", User.Identity.Name);

                        if (successDOT == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                    }
                    else
                    {
                        //LetterControllerLOCAL lc = new LetterControllerLOCAL(_clinContext, _docContext);
                        Referral refer = await _referralData.GetReferralDetails(refID);
                        _lc.DoPDF(letterID, mpi, refID, User.Identity.Name, refer.ReferrerCode, "", "", 0, "", false, false, diaryID, freeText1, "", 0,
                            "", "", null, false, "", leafletID);
                    }

                    if (successDiary == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canDiaryUpdate(SQL)" }); }
                }
            }
                        
            
            if (toBeReviewedBy == null) { toBeReviewedBy = ""; }//because the default value isn't being assigned for some reason!            

            if(clinician != null && clinician != "" && _ivm.icpCancer.WaitingListVenue == null)
            {  
                int successWL = _crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, clinic, "Cancer", clinician, comments,
                    User.Identity.Name, null, null, false, false); //where is "not for cross booking" stored?

                if (successWL == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canAddWL(SQL)" }); }
            }

            //if(finalReview == "Yes")
            //{
                //finalReviewText = reviewText;
            reviewBy = user.STAFF_CODE;
                //finalReviewDate = DateTime.Today;
            //}

            int success = _crud.CallStoredProcedure("ICP Cancer", "ICP Review", id, letter.GetValueOrDefault(), 0, reviewBy, finalReview, toBeReviewedBy, addNotes,
                    User.Identity.Name, null, null, false, false, 0,0,0,clinician, clinic, comments);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canReview" }); }

            if(finalReview == "Yes")
            {
                int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", "REPSUM", "", "", User.Identity.Name, null, null, false, false);
                var diary = await _diaryData.GetLatestDiaryByRefID(refID, "REPSUM");
                int diaryID = diary.DiaryID;

                //LetterControllerLOCAL let = new LetterControllerLOCAL(_clinContext, _docContext);
                //let.DoRepsum(_ivm.icpCancer.ICP_Cancer_ID, diaryID, User.Identity.Name);
                //_lc.DoRepsum(_ivm.icpCancer.ICP_Cancer_ID, diaryID, User.Identity.Name);
                return RedirectToAction("PrepareRepsum", "Repsum", new { id = id, diaryID = diaryID }); 
                //we HAVE to do it this way or it won't update the data model
            }

            return RedirectToAction("CancerReview", new { id = id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FurtherRequest(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;

                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Cancer Post Clinic Letter", "ID=" + id.ToString());
                _ivm.icpCancer = await _triageData.GetCancerICPDetails(id);
                _ivm.cancerRequestsList = await _cancerRequestData.GetCancerRequestsList();
                _ivm.relatives = await _relativeData.GetRelativesList(_ivm.icpCancer.MPI);
                _ivm.clinicians = await _clinicianData.GetClinicianList();
                _ivm.specialities = await _clinicianData.GetClinicianTypeList();
                _ivm.relativesDiagnoses = new List<RelativesDiagnosis>();
                
                if (_ivm.relatives.Count > 0)
                {
                    foreach (var rel in _ivm.relatives)
                    {
                        foreach (var diag in await _relDiagData.GetRelativeDiagnosisList(rel.relsid))
                        {
                            _ivm.relativesDiagnoses.Add(diag);
                        }
                    }
                }
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-postclinicletter" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FurtherRequest(int id, int request, string? freeText = "", int? relID = 0, string? clinicianCode = "", string? siteText = "",
            string? freeText1="", string? freeText2="", string? additionalText="", DateTime? diagDate = null, bool? isPreview=false)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;

                _ivm.cancerRequest = await _cancerRequestData.GetCancerRequestDetail(request);
                _ivm.icpCancer = await _triageData.GetCancerICPDetails(id);
                var icpDetails = await _triageData.GetICPDetails(_ivm.icpCancer.ICPID);               
                //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
                int mpi = icpDetails.MPI;
                int refID = icpDetails.REFID;
                int docID = _ivm.cancerRequest.DocContentID.GetValueOrDefault();
                int docID2 = _ivm.cancerRequest.DocContentID2.GetValueOrDefault();
                int docID3 = _ivm.cancerRequest.DocContentID3.GetValueOrDefault();

                if(freeText != "") { freeText1 = freeText; }

                if (docID != null && docID != 0)
                {
                    var doc = await _documentsData.GetDocumentDetails(docID);
                    string docCode = doc.DocCode;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    int diaryID = diary.DiaryID;

                    var refer = await _referralData.GetReferralDetails(refID);
                    _lc.DoPDF(docID, mpi, refID, User.Identity.Name, refer.ReferrerCode, additionalText, "", 0, "",
                        false, false, 0, freeText1, freeText2, relID, clinicianCode, siteText, diagDate, isPreview);
                }

                if (docID2 != null && docID2 != 0)
                {
                    var doc = await _documentsData.GetDocumentDetails(docID2);
                    string docCode = doc.DocCode;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    int diaryID = diary.DiaryID;

                    var refer = await _referralData.GetReferralDetails(refID);
                    _lc.DoPDF(docID2, mpi, refID, User.Identity.Name, refer.ReferrerCode, additionalText, "", 0, "",
                        false, false, 0, freeText1, freeText2, relID, clinicianCode, siteText, diagDate, isPreview);
                }

                if (docID3 != null && docID3 != 0)
                {
                    var doc = await _documentsData.GetDocumentDetails(docID3);
                    string docCode = doc.DocCode;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    int diaryID = diary.DiaryID;

                    var refer = await _referralData.GetReferralDetails(refID);
                    _lc.DoPDF(docID3, mpi, refID, User.Identity.Name, refer.ReferrerCode, additionalText, "", 0, "",
                        false, false, 0, freeText1, freeText2, relID, clinicianCode, siteText, diagDate, isPreview);
                }
                if (!isPreview.GetValueOrDefault())
                {
                    return RedirectToAction("CancerReview", new { id = id, message="Letter submitted for filing/printing in EDMS", success=true });
                }
                else
                {
                    return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-postclinicletter" });
            }
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RiskAndSurveillance(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk and Surveillance", "ID=" + id.ToString(), _ip.GetIPAddress());

                _ivm.riskDetails = await _riskData.GetRiskDetails(id);
                var pat = await _referralData.GetReferralDetails(_ivm.riskDetails.RefID);
                int mpi = pat.MPI;
                _ivm.surveillanceList = await _survData.GetSurveillanceList(mpi);
                _ivm.surveillanceList = _ivm.surveillanceList.Where(s => s.RiskID == id).ToList();

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canRiskSurv" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeGeneralTriage(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Change General Triage", "ID=" + id.ToString(), _ip.GetIPAddress());
                _ivm.triage = await _triageData.GetTriageDetails(id);
                _ivm.icpGeneral = await _triageData.GetGeneralICPDetailsByICPID(id);

                var staffList = await _staffUser.GetClinicalStaffList();
                _ivm.consultants = staffList.Where(s => s.CLINIC_SCHEDULER_GROUPS == "Consultant").ToList();
                _ivm.GCs = staffList.Where(s => s.CLINIC_SCHEDULER_GROUPS == "GC").ToList();
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-changeGenTriage" });
            }
        }

        [HttpPost]
        public IActionResult ChangeGeneralTriage(int icpId, string newConsultant, string newGC)
        {
            try
            {
                if(newConsultant == null) { newConsultant = ""; }
                int success = _crud.CallStoredProcedure("ICP General", "Change", icpId, 0, 0, newConsultant, newGC, "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-changeGenTriage(SQL)" }); }

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-changeGenTriage" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeTriagePathway(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Change Triage Pathway", "ID=" + id.ToString(), _ip.GetIPAddress());

                _ivm.triage = await _triageData.GetTriageDetails(id);
                _ivm.pathways = await _pathwayData.GetPathwayList();                

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-changePathway" });
            }
        }

        [HttpPost]
        public IActionResult ChangeTriagePathway(int id, string newPathway)
        {
            try
            {
                int success = _crud.CallStoredProcedure("ICP Cancer", "Change Pathway", id, 0, 0, newPathway, "", "", "", User.Identity.Name);
                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-changePathway(SQL)" }); }

                return RedirectToAction("ICPDetails", "Triage", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-changePathway" });
            }
        }



        [HttpPost]
        public IActionResult ReturnToConsultant(int icpId)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("ICP General", "Return", icpId, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-returnToCons" }); }

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveIndicationNotes(int icpID, string indicationNotes)
        {
            try
            {
                int success = _crud.CallStoredProcedure("ICP General", "Indication Notes", icpID, 0, 0, "", "", "", indicationNotes, User.Identity.Name);
                
                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-returnToCons(SQL)" }); }                

                return RedirectToAction("ICPDetails", "Triage", new { id = icpID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-returnToCons" });
            }
        }           
    }
}
