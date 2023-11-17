using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class TriageController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly DocumentContext _docContext;
        private readonly IConfiguration _config;

        public TriageController(ClinicalContext context, DocumentContext docContext, IConfiguration config)
        {
            _context = context;
            _docContext = docContext;
            _config = config;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {            
            ICPVM ivm = new ICPVM();
            VMData vm = new VMData(_context);
            ivm.triages = vm.GetTriageList(User.Identity.Name);
            ivm.icpCancerList = vm.GetCancerICPList(User.Identity.Name);                       
            return View(ivm);
        }

        [Authorize]
        public async Task<IActionResult> ICPDetails(int? id)
        {
            var triages = await _context.Triages.FirstOrDefaultAsync(t => t.ICPID == id);

            if (triages == null)
            {
                return RedirectToAction("NotFound", "WIP");
            }

            ICPVM ivm = new ICPVM();
            VMData vm = new VMData(_context);
            ivm.triage = vm.GetTriageDetails(id);
            ivm.clinicalFacilityList = vm.GetClinicalFacilities();
            ivm.icpGeneral = vm.GetGeneralICPDetails(id);
            ivm.icpCancer = vm.GetCancerICPDetails(id);
            ivm.cancerActionsList = vm.GetICPCancerActionsList();
            ivm.generalActionsList = vm.GetICPGeneralActionsList();
            ivm.generalActionsList2 = vm.GetICPGeneralActionsList2();            
            return View(ivm);
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int iIcpID, string? sFacility, int? iDuration, string? sComment, bool isSPR, bool isChild, int? iTP, int? iTP2)
        {
            var icp = await _context.Triages.FirstOrDefaultAsync(i => i.ICPID == iIcpID);            
            var referral = await _context.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
            var staffmember = await _context.StaffMembers.FirstOrDefaultAsync(s => s.EMPLOYEE_NUMBER == User.Identity.Name);
            int iMPI = icp.MPI;
            string sReferrer = referral.ReferrerCode;
            string sApptIntent = "";
            string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;

            
            if (sComment==null)
            {
                sComment = "";
            }

            if(iTP2 == 3)
            {
                sApptIntent = "CLICS";
            }

            CRUD crud = new CRUD(_config);
            

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

            if(sFacility != null && sFacility != "")
            {
                crud.CallStoredProcedure("Waiting List", "Create", iMPI, 0, 0, sFacility, "General", "", sComment, User.Identity.Name);                
            }

            if (iTP2 == 2) //CTB letter
            {
                LetterController lc = new LetterController(_docContext);
                lc.DoPDF(184, iMPI, referral.refid, User.Identity.Name, sReferrer);
            }

            if (iTP2 == 3) //CLICS
            {

            }

            if (iTP2 == 7) //Reject letter
            {

            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int iIcpID, int iAction)
        {
            var icp = await _context.Triages.FirstOrDefaultAsync(i => i.ICPID == iIcpID);
            int iMPI = icp.MPI;
            int iRefID = icp.RefID;
            var referral = await _context.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
            string sReferrer = referral.ReferrerCode;

            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("ICP Cancer", "Triage", iIcpID, iAction, 0, "", "", "", "", User.Identity.Name);
            
            if(iAction == 5)
            {
                LetterController lc = new LetterController(_docContext);
                lc.DoPDF(156, iMPI, iRefID, User.Identity.Name, sReferrer);
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CancerReview(int id)
        {            
            if(id == null)
            {
                return RedirectToAction("NotFound", "WIP");
            }

            ICPVM ivm = new ICPVM();
            VMData vm = new VMData(_context);
            ivm.clinicalFacilityList = vm.GetClinicalFacilities();
            ivm.staffMembers = vm.GetClinicians();
            ivm.icpCancer = vm.GetCancerICPDetails(id);
            ivm.riskList = vm.GetRiskList(id);
            ivm.surveillanceList = vm.GetSurveillanceList(id);
            ivm.eligibilityList = vm.GetTestingEligibilityList(id);
            return View(ivm);            
        }

        [HttpPost]
        public async Task<IActionResult> CancerReview(int id, string sComments)
        {
            return RedirectToAction("NotFound", "WIP");
        }

        [HttpGet]
        public async Task<IActionResult> RiskAndSurveillance(int id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "WIP");
            }

            ICPVM ivm = new ICPVM();
            VMData vm = new VMData(_context);
            ivm.riskDetails = vm.GetRiskDetails(id);
            var icp = _context.ICPCancer.FirstOrDefault(i => i.ICPID == ivm.riskDetails.ICPID);
            ivm.surveillanceList = vm.GetSurveillanceList(icp.ICP_Cancer_ID).Where(s => s.RiskID == id).ToList();
            
            return View(ivm);
        }

        
    }
}
