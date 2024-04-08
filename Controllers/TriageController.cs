using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using System.Data;
using ClinicX.Meta;
using ClinicX.Models;

namespace ClinicX.Controllers
{
    public class TriageController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;
        private readonly ICPVM _ivm;
        private readonly VMData _vm;
        private readonly VMData _vmDoc;
        private readonly CRUD _crud;
        private readonly LetterController _lc;


        public TriageController(ClinicalContext clinContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            _ivm = new ICPVM();
            _vm = new VMData(_clinContext);
            _vmDoc = new VMData(_docContext);
            _crud = new CRUD(_config);
            _lc = new LetterController(_clinContext, _docContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                _ivm.staffCode = _vm.GetCurrentStaffUser(User.Identity.Name).STAFF_CODE;
                _ivm.triages = _vm.GetTriageList(User.Identity.Name);
                _ivm.icpCancerListOwn = _vm.GetCancerICPList(User.Identity.Name).Where(r => r.GC_CODE == _ivm.staffCode).ToList();
                _ivm.icpCancerListOther = _vm.GetCancerICPList(User.Identity.Name).Where(r => r.ToBeReviewedby == User.Identity.Name.ToUpper()).ToList();
                int bleep = _ivm.icpCancerListOwn.Count();                
                int bloop = _ivm.icpCancerListOther.Count();
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> ICPDetails(int? id)
        {
            try
            {
                //var triages = await _clinContext.Triages.FirstOrDefaultAsync(t => t.ICPID == id);

                _ivm.triage = _vm.GetTriageDetails(id);

                if (_ivm.triage == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                //_ivm.triage = _vm.GetTriageDetails(id);
                _ivm.referralDetails = _vm.GetReferralDetails(_ivm.triage.RefID);
                _ivm.clinicalFacilityList = _vm.GetClinicalFacilitiesList();
                _ivm.icpGeneral = _vm.GetGeneralICPDetails(id);
                _ivm.icpCancer = _vm.GetCancerICPDetails(id);
                _ivm.cancerActionsList = _vm.GetICPCancerActionsList();
                _ivm.generalActionsList = _vm.GetICPGeneralActionsList();
                _ivm.generalActionsList2 = _vm.GetICPGeneralActionsList2();
                _ivm.loggedOnUserType = _vm.GetCurrentStaffUser(User.Identity.Name).CLINIC_SCHEDULER_GROUPS;
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int icpID, string? facility, int? duration, string? comment, bool isSPR, bool isChild, int? tp, int? tp2c, int? tp2nc)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == icpID);
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                var staffmember = await _clinContext.StaffMembers.FirstOrDefaultAsync(s => s.EMPLOYEE_NUMBER == User.Identity.Name);
                int mpi = icp.MPI;
                int tp2;
                string referrer = referral.ReferrerCode;
                string sApptIntent = "";
                string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;

                if (comment == null)
                {
                    comment = "";
                }

                if (tp2c != null)
                {
                    tp2 = tp2c.GetValueOrDefault();
                }
                else
                {
                    tp2 = tp2nc.GetValueOrDefault();
                }

                if (tp2 == 3)
                {
                    sApptIntent = "CLICS";
                }
  
                if (sStaffType == "Consultant")
                {
                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), 0,
                        facility, sApptIntent, "", comment, User.Identity.Name, null, null, isSPR, isChild, duration);

                        if (success == 0) { return RedirectToAction("Index", "WIP"); }
                        //_crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, facility, "General", "", comment, User.Identity.Name);

                        //_lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer);
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), 0,
                        "", sApptIntent, "", comment, User.Identity.Name);

                        if (success == 0) { return RedirectToAction("Index", "WIP"); }
                    }
                }
                else
                {
                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, 0, tp2,
                        facility, sApptIntent, "", comment, User.Identity.Name, null, null, isSPR, isChild, duration);

                        if (success == 0) { return RedirectToAction("Index", "WIP"); }
                        //_crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, facility, "General", "", comment, User.Identity.Name);

                        //_lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer);
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, 0, tp2,
                        "", sApptIntent, "", comment, User.Identity.Name);

                        if (success == 0) { return RedirectToAction("Index", "WIP"); }
                    }
                }

                if (facility != null && facility != "")
                {
                    int success = _crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, facility, "General", "", comment, User.Identity.Name);

                    if (success == 0) { return RedirectToAction("Index", "WIP"); }
                }

                if (tp2 == 2) //CTB letter
                {
                    //LetterController _lc = new LetterController(_docContext);
                    _lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer);
                }                

                if (tp2 == 7) //Reject letter
                {
                    //LetterController _lc = new LetterController(_docContext);
                    _lc.DoPDF(208, mpi, referral.refid, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int icpID, int action)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == icpID);
                int mpi = icp.MPI;
                int refID = icp.RefID;
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                string referrer = referral.ReferrerCode;

                CRUD _crud = new CRUD(_config);
                int success = _crud.CallStoredProcedure("ICP Cancer", "Triage", icpID, action, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                if (action == 5)
                {
                    LetterController _lc = new LetterController(_clinContext, _docContext);
                    _lc.DoPDF(156, mpi, refID, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancerReview(int id)
        {
            try
            {
                if (id == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                
                _ivm.clinicalFacilityList = _vm.GetClinicalFacilitiesList();
                _ivm.staffMembers = _vm.GetClinicalStaffList();
                _ivm.icpCancer = _vm.GetCancerICPDetails(id);
                _ivm.riskList = _vm.GetRiskList(id);
                _ivm.surveillanceList = _vm.GetSurveillanceList(_ivm.icpCancer.MPI);
                _ivm.eligibilityList = _vm.GetTestingEligibilityList(_ivm.icpCancer.MPI);
                _ivm.documentList = _vmDoc.GetDocumentsList().Where(d => (d.DocCode.StartsWith("O") && d.DocGroup == "Outcome") || d.DocCode.Contains("PrC")).ToList();
                _ivm.cancerReviewActionsLists = _vm.GetICPCancerReviewActionsList();
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancerReview(int id, string finalReview, string? clinician = "", string? clinic = "", string? comments = "", 
            string? addNotes = "", bool? isNotForCrossBooking = false, int? letter = 0)
        {
            //bool isfinalReview = false;
            _ivm.icpCancer = _vm.GetCancerICPDetails(id);
            var icpDetails = _vm.GetICPDetails(_ivm.icpCancer.ICPID);
            string reviewText = "";
            string finalReviewText = "";
            string finalReviewBy = "";
            _ivm.cancerReviewActionsLists = _vm.GetICPCancerReviewActionsList();
            //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
            int mpi = icpDetails.MPI;
            int refID = icpDetails.REFID;

            if (letter != null && letter != 0)
            {                
                _ivm.cancerAction = _vm.GetICPCancerAction(letter.GetValueOrDefault());
                string docCode = _ivm.cancerAction.DocCode;

                if (letter != 1 && letter != 11)
                {
                    reviewText = docCode;
                    
                    if (reviewText != null)
                    {
                        reviewText = reviewText + " letter on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + _vm.GetCurrentStaffUser(User.Identity.Name).NAME;
                    }
                }
                    string diaryText = "";
                int letterID = _vmDoc.GetDocumentDetailsByDocCode(docCode).DocContentID;
                
                _lc.DoPDF(letterID, mpi, refID, User.Identity.Name, _vm.GetReferralDetails(refID).ReferringClinician);
                int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", diaryText, User.Identity.Name, null, null, false, false);

                if (successDiary == 0) { return RedirectToAction("Index", "WIP"); }
            }
            

            if(clinician != null && clinician != "")
            {                
                int successWL = _crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, clinic, "Cancer", clinician, comments,
                    User.Identity.Name, null, null, false, false); //where is "not for cross booking" stored?

                if (successWL == 0) { return RedirectToAction("Index", "WIP"); }
            }

            if(finalReview == "Yes")
            {
                finalReviewText = reviewText;
                finalReviewBy = _vm.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                //finalReviewDate = DateTime.Today;
            }

            int success = _crud.CallStoredProcedure("ICP Cancer", "ICP Review", id, letter.GetValueOrDefault(), 0, finalReviewBy, finalReviewText, "", addNotes,
                    User.Identity.Name, null, null, false, false);

            if (success == 0) { return RedirectToAction("Index", "WIP"); }

            return RedirectToAction("Index");            
        }

        [HttpGet]
        public async Task<IActionResult> RiskAndSurveillance(int id)
        {
            try
            {
                if (id == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                
                _ivm.riskDetails = _vm.GetRiskDetails(id);
                int mpi = _vm.GetReferralDetails(_ivm.riskDetails.RefID).MPI;               
                _ivm.surveillanceList = _vm.GetSurveillanceList(mpi).Where(s => s.RiskID == id).ToList();

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeGeneralTriage(int id)
        {
            try
            {
                _ivm.icpGeneral = _vm.GetGeneralICPDetails(id);
                _ivm.consultants = _vm.GetClinicalStaffList().Where(s => s.CLINIC_SCHEDULER_GROUPS == "Consultant").ToList();
                _ivm.GCs = _vm.GetClinicalStaffList().Where(s => s.CLINIC_SCHEDULER_GROUPS == "GC").ToList();
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGeneralTriage(int icpId, string newConsultant, string newGC)
        {
            try
            {
                if(newConsultant == null) 
                {
                    newConsultant = "";
                }
                int success = _crud.CallStoredProcedure("ICP General", "Change", icpId, 0, 0, newConsultant, newGC, "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnToConsultant(int icpId)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("ICP General", "Return", icpId, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveIndicationNotes(int icpID, string indicationNotes)
        {
            try
            {
                int success = _crud.CallStoredProcedure("ICP General", "Indication Notes", icpID, 0, 0, "", "", "", indicationNotes, User.Identity.Name);
                
                if (success == 0) { return RedirectToAction("Index", "WIP"); }                
                

                return RedirectToAction("ICPDetails", "Triage", new { id = icpID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
