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
        private readonly ICPVM ivm;
        private readonly VMData vm;
        private readonly VMData vmDoc;
        private readonly CRUD crud;
        private readonly LetterController lc;


        public TriageController(ClinicalContext clinContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            ivm = new ICPVM();
            vm = new VMData(_clinContext);
            vmDoc = new VMData(_docContext);
            crud = new CRUD(_config);
            lc = new LetterController(_clinContext, _docContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {               
                ivm.triages = vm.GetTriageList(User.Identity.Name);
                ivm.icpCancerList = vm.GetCancerICPList(User.Identity.Name);
                return View(ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> ICPDetails(int? id)
        {
            try
            {
                //var triages = await _clinContext.Triages.FirstOrDefaultAsync(t => t.ICPID == id);

                var triage = vm.GetTriageDetails(id);

                if (triage == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                
                ivm.triage = vm.GetTriageDetails(id);
                ivm.clinicalFacilityList = vm.GetClinicalFacilitiesList();
                ivm.icpGeneral = vm.GetGeneralICPDetails(id);
                ivm.icpCancer = vm.GetCancerICPDetails(id);
                ivm.cancerActionsList = vm.GetICPCancerActionsList();
                ivm.generalActionsList = vm.GetICPGeneralActionsList();
                ivm.generalActionsList2 = vm.GetICPGeneralActionsList2();
                return View(ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int iIcpID, string? sFacility, int? iDuration, string? sComment, bool isSPR, bool isChild, int? iTP, int? iTP2)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == iIcpID);
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                var staffmember = await _clinContext.StaffMembers.FirstOrDefaultAsync(s => s.EMPLOYEE_NUMBER == User.Identity.Name);
                int iMPI = icp.MPI;
                string sReferrer = referral.ReferrerCode;
                string sApptIntent = "";
                string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;

                if (sComment == null)
                {
                    sComment = "";
                }

                if (iTP2 == 3)
                {
                    sApptIntent = "CLICS";
                }
  
                if (sStaffType == "Consultant")
                {
                    if (sFacility != null && sFacility != "") // && sClinician != null && sClinician != "")
                    {
                        crud.CallStoredProcedure("ICP General", "Triage", iIcpID, iTP.GetValueOrDefault(), 0,
                        sFacility, sApptIntent, "", sComment, User.Identity.Name, null, null, isSPR, isChild, iDuration);

                        //crud.CallStoredProcedure("Waiting List", "Create", iMPI, 0, 0, sFacility, "General", "", sComment, User.Identity.Name);

                        //lc.DoPDF(184, iMPI, referral.refid, User.Identity.Name, sReferrer);
                    }
                    else
                    {
                        crud.CallStoredProcedure("ICP General", "Triage", iIcpID, iTP.GetValueOrDefault(), 0,
                        "", sApptIntent, "", sComment, User.Identity.Name);
                    }
                }
                else
                {
                    if (sFacility != null && sFacility != "") // && sClinician != null && sClinician != "")
                    {
                        crud.CallStoredProcedure("ICP General", "Triage", iIcpID, 0, iTP2.GetValueOrDefault(),
                        sFacility, sApptIntent, "", sComment, User.Identity.Name, null, null, isSPR, isChild, iDuration);

                        //crud.CallStoredProcedure("Waiting List", "Create", iMPI, 0, 0, sFacility, "General", "", sComment, User.Identity.Name);

                        //lc.DoPDF(184, iMPI, referral.refid, User.Identity.Name, sReferrer);
                    }
                    else
                    {
                        crud.CallStoredProcedure("ICP General", "Triage", iIcpID, 0, iTP2.GetValueOrDefault(),
                        "", sApptIntent, "", sComment, User.Identity.Name);
                    }
                }

                if (sFacility != null && sFacility != "")
                {
                    crud.CallStoredProcedure("Waiting List", "Create", iMPI, 0, 0, sFacility, "General", "", sComment, User.Identity.Name);
                }

                if (iTP2 == 2) //CTB letter
                {
                    //LetterController lc = new LetterController(_docContext);
                    lc.DoPDF(184, iMPI, referral.refid, User.Identity.Name, sReferrer);
                }                

                if (iTP2 == 7) //Reject letter
                {
                    //LetterController lc = new LetterController(_docContext);
                    lc.DoPDF(208, iMPI, referral.refid, User.Identity.Name, sReferrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int iIcpID, int iAction)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == iIcpID);
                int iMPI = icp.MPI;
                int iRefID = icp.RefID;
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                string sReferrer = referral.ReferrerCode;

                CRUD crud = new CRUD(_config);
                crud.CallStoredProcedure("ICP Cancer", "Triage", iIcpID, iAction, 0, "", "", "", "", User.Identity.Name);

                if (iAction == 5)
                {
                    LetterController lc = new LetterController(_clinContext, _docContext);
                    lc.DoPDF(156, iMPI, iRefID, User.Identity.Name, sReferrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
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
                
                ivm.clinicalFacilityList = vm.GetClinicalFacilitiesList();
                ivm.staffMembers = vm.GetClinicalStaffList();
                ivm.icpCancer = vm.GetCancerICPDetails(id);
                ivm.riskList = vm.GetRiskList(id);
                ivm.surveillanceList = vm.GetSurveillanceList(ivm.icpCancer.MPI);
                ivm.eligibilityList = vm.GetTestingEligibilityList(ivm.icpCancer.MPI);
                ivm.documentList = vmDoc.GetDocumentsList().Where(d => (d.DocCode.StartsWith("O") && d.DocGroup == "Outcome") || d.DocCode.Contains("PrC")).ToList();
                return View(ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancerReview(int id, string sFinalReview, string? sClinician = "", 
            string? sClinic = "", string? sComments = "", string? sAddNotes = "", bool? isNotForCrossBooking = false,
            int? iLetter = 0)
        {
            bool isFinalReview = false;
            string sFinalReviewBy = "";
            ivm.cancerReviewActionsLists = vm.GetICPCancerReviewActionsList();
            DateTime dFinalReviewDate = DateTime.Parse("1900-01-01");
            int iMPI = vm.GetICPDetails(vm.GetCancerICPDetails(id).ICPID).MPI;
            int iRefID = vm.GetICPDetails(vm.GetCancerICPDetails(id).ICPID).REFID;

            if (iLetter != 0)
            {
                ivm.cancerAction = vm.GetICPCancerAction(iLetter.GetValueOrDefault());
                int iLetterID = vmDoc.GetDocumentDetailsByDocCode(ivm.cancerAction.DocCode).DocContentID;
                
                lc.DoPDF(iLetterID, iMPI, iRefID, User.Identity.Name, vm.GetReferralDetails(iRefID).ReferringClinician);
            }

            if(sClinician != "")
            {                
                crud.CallStoredProcedure("Waiting List", "Create", iMPI, 0, 0, sClinic, "Cancer", sClinician, sComments,
                    User.Identity.Name, null, null, false, false); //where is "not for cross booking" stored?
            }

            if(sFinalReview == "Yes")
            {
                isFinalReview = true;
                sFinalReviewBy = vm.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                dFinalReviewDate = DateTime.Today;
            }

            //do the CRUD
            return RedirectToAction("NotFound", "WIP");
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
                
                ivm.riskDetails = vm.GetRiskDetails(id);
                int iMPI = vm.GetReferralDetails(ivm.riskDetails.RefID).MPI;               
                ivm.surveillanceList = vm.GetSurveillanceList(iMPI).Where(s => s.RiskID == id).ToList();

                return View(ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeGeneralTriage(int id)
        {
            try
            {
                ivm.icpGeneral = vm.GetGeneralICPDetails(id);
                ivm.consultants = vm.GetClinicalStaffList().Where(s => s.CLINIC_SCHEDULER_GROUPS == "Consultant").ToList();
                ivm.GCs = vm.GetClinicalStaffList().Where(s => s.CLINIC_SCHEDULER_GROUPS == "GC").ToList();
                return View(ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGeneralTriage(int icpId, string sNewConsultant, string sNewGC)
        {
            try
            {
                if(sNewConsultant == null) 
                {
                    sNewConsultant = "";
                }
                crud.CallStoredProcedure("ICP General", "Change", icpId, 0, 0, sNewConsultant, sNewGC, "", "", User.Identity.Name);

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnToConsultant(int icpId)
        {
            try
            {                
                crud.CallStoredProcedure("ICP General", "Return", icpId, 0, 0, "", "", "", "", User.Identity.Name);

                return RedirectToAction("ICPDetails", "Triage", new { id = icpId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
