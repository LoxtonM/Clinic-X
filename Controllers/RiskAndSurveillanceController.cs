﻿using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RiskAndSurveillanceController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly RiskSurveillanceVM _rsvm;
        private readonly IConfiguration _config;        
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly ITriageData _triageData;
        private readonly IRiskData _riskData;
        private readonly ISurveillanceData _survData;
        private readonly ITestEligibilityData _testEligibilityData;
        private readonly IMiscData _misc;
        private readonly IGeneChangeData _gene;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public RiskAndSurveillanceController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rsvm = new RiskSurveillanceVM();
            _patientData = new PatientData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _triageData = new TriageData(_clinContext);
            _riskData = new RiskData(_clinContext);
            _survData = new SurveillanceData(_clinContext);
            _testEligibilityData = new TestEligibilityData(_clinContext);
            _misc = new MiscData(_config);
            _gene = new GeneChangeData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }

        public IActionResult RiskDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString());
                _rsvm.GeneChange = _gene.GetGeneChangeList();
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.surveillanceList = _survData.GetSurveillanceListByRiskID(_rsvm.riskDetails.RiskID);
                _rsvm.eligibilityList = _testEligibilityData.GetTestingEligibilityList(_rsvm.riskDetails.MPI);
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpGet]
        public IActionResult SurvDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + id.ToString());
                _rsvm.GeneChange= _gene.GetGeneChangeList();
                _rsvm.surveillanceDetails = _survData.GetSurvDetails(id);
                int mpi = _rsvm.surveillanceDetails.MPI;

                _rsvm.patient = _patientData.GetPatientDetails(mpi);
                
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv" });
            }
        }

        [HttpPost]
        public IActionResult SurvDetails(int survID, int geneChange)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Risk Details", "ID=" + survID.ToString());

                int riskID = _survData.GetSurvDetails(survID).RiskID;
                

                int success = _crud.CallStoredProcedure("Surveillance", "Add Gene Change", survID, geneChange, 0, "", "", "", "",
                    User.Identity.Name, null, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addRisk(QSL)" }); }
                               
                
                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = riskID });               
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "SurvDetails" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> AddNewRisk(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Risk", "ICPID=" + id.ToString());

                _rsvm.icpCancer = _triageData.GetCancerICPDetailsByICPID(id);
                _rsvm.patient = _patientData.GetPatientDetails(_rsvm.icpCancer.MPI);
                _rsvm.refID = _triageData.GetICPDetails(id).REFID;
                _rsvm.riskCodes = _riskData.GetRiskCodesList();
                _rsvm.survSiteCodes = _survData.GetSurvSiteCodesList();
                _rsvm.staffMembersList = _staffUser.GetClinicalStaffList();
                _rsvm.staffCode = staffCode;
                _rsvm.calculationTools = _riskData.GetCalculationToolsList();
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addRisk" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewRisk(int refID, string riskCode, string siteCode, string clinCode, 
            DateTime riskDate, float lifetimePercent, string comments, float f2529, float f3040, float f4050, 
            float f5060, bool isUseLetter, string tool)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Risk", "Create", refID, 0, 0, riskCode, siteCode, clinCode, comments,
                    User.Identity.Name, riskDate, null, isUseLetter, false, 0, 0, 0, tool, "", "", lifetimePercent,
                    f2529, f3040, f4050, f5060);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addRisk(QSL)" }); }

                int riskID = _misc.GetRiskID(refID);
                int icpID = _riskData.GetRiskDetails(riskID).ICPID;
                int icpCancerID = _triageData.GetCancerICPDetailsByICPID(icpID).ICP_Cancer_ID;
                return RedirectToAction("CancerReview", "Triage", new { id = icpCancerID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addRisk" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNewSurveillance(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Surveillance", "RiskID=" + id.ToString());

                _rsvm.riskID = id;
                _rsvm.riskDetails = _riskData.GetRiskDetails(id);
                _rsvm.patient = _patientData.GetPatientDetails(_rsvm.riskDetails.MPI);
                _rsvm.survSiteCodes = _survData.GetSurvSiteCodesList();
                _rsvm.survTypeCodes = _survData.GetSurvTypeCodesList();
                _rsvm.survFreqCodes = _survData.GetSurvFreqCodesList();
                _rsvm.discontinuedReasonCodes = _survData.GetDiscReasonCodesList();
                _rsvm.staffMembersList = _staffUser.GetClinicalStaffList();
                
                return View(_rsvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addSurv" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewSurveillance(int riskID, string siteCode, string typeCode, string clinCode, 
            DateTime recDate, int startAge, int endAge, string frequency, bool isUseLetter, bool isYN, string? discReason)
        {
            try
            {
                if(discReason == null)
                {
                    discReason = ""; //because it can't simply evaluate the optional parameter as an empty string for some reason!!!
                }
                
                int success = _crud.CallStoredProcedure("Surveillance", "Create", riskID, startAge, endAge, siteCode, typeCode, clinCode, "",
                    User.Identity.Name, recDate, null, isUseLetter, isYN, 0, 0, 0, frequency, discReason);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RiskSurv-addSurv(SQL)" }); }

                return RedirectToAction("RiskDetails", "RiskAndSurveillance", new { id = riskID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RiskSurv-addSurv" });
            }
        }
    }
}
