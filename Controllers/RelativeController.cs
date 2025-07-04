﻿using ClinicalXPDataConnections.Data;
using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicX.Meta;
using APIControllers.Controllers;
using APIControllers.Data;

namespace ClinicX.Controllers
{
    public class RelativeController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cXContext;
        private readonly DocumentContext _docContext;
        private readonly APIContext _apiContext;
        private readonly RelativeDiagnosisVM _rdvm;
        private readonly RelativeVM _rvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly APIController _api;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public RelativeController(ClinicalContext context, ClinicXContext cXContext, DocumentContext docContext, APIContext apiContext, IConfiguration config)
        {
            _clinContext = context;
            _cXContext = cXContext;
            _docContext = docContext;
            _apiContext = apiContext;
            _config = config;
            _crud = new CRUD(_config);
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _rdvm = new RelativeDiagnosisVM();
            _rvm = new RelativeVM();
            _api = new APIController(_apiContext, _config);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public IActionResult Index()
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Relatives", "", _ip.GetIPAddress());

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - View Relative", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID).MPI;
                
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDetails" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Relative", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID).MPI;
                _rdvm.relationList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _relativeData.GetGenderList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string forename1,
            string forename2, string surname, string relation, string dob, string dod,
            int isAffected, string sex)
        {
            try
            {
                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);

                //making sure all the nulls have values

                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (dob != null)
                {
                    birthDate = DateTime.Parse(dob);
                }
                else
                {
                    birthDate = DateTime.Parse("1/1/1900");
                }

                if (dod != null)
                {
                    deathDate = DateTime.Parse(dod);
                }
                else
                {
                    deathDate = DateTime.Parse("1/1/1900");
                }

                if (title == null) { title = ""; }

                if (forename2 == null) { forename2 = ""; }

                int success = _crud.CallStoredProcedure("Relative", "Edit", id, isAffected, 0, title, forename1, forename2, surname,
                        User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-edit(SQL)" }); }

                return RedirectToAction("RelativeDetails", "Relative", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-edit" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int wmfacsid)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Add Relative", "WMFACSID=" + wmfacsid.ToString(), _ip.GetIPAddress());

                _rdvm.WMFACSID = wmfacsid;
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                _rdvm.relationList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _relativeData.GetGenderList();
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int wmfacsid, string title, string forename1, 
            string forename2, string surname, string relation, string sDOB, string sDOD, 
            int isAffected, string sex)
        {
            try
            {
                _rdvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (sDOB != null)
                {
                    birthDate = DateTime.Parse(sDOB);
                }
                else
                {
                    birthDate = DateTime.Parse("1/1/1900");
                }

                if (sDOD != null)
                {
                    deathDate = DateTime.Parse(sDOD);
                }
                else
                {
                    deathDate = DateTime.Parse("1/1/1900");
                }

                if (forename2 == null) { forename2 = ""; }

                int success = _crud.CallStoredProcedure("Relative", "Create", wmfacsid, isAffected, 0, title, forename1, forename2, surname,
                    User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-add(SQL)" }); }

                var patient = _patientData.GetPatientDetailsByWMFACSID(wmfacsid);

                return RedirectToAction("PatientDetails", "Patient", new { id = _rdvm.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImportRelatives(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Import Relatives", "WMFACSID=" + id.ToString(), _ip.GetIPAddress());
                _rvm.patient = _patientData.GetPatientDetailsByWMFACSID(id);
                _rvm.cgudbRelativesList = _relativeData.GetRelativesList(_rvm.patient.MPI);

                List<APIControllers.Models.Relative> ptRels = new List<APIControllers.Models.Relative>();

                TestAPIController apitest = new TestAPIController(_apiContext, _config);
                ptRels = await apitest.ImportRelativesFromPhenotips(_rvm.patient.MPI);

                _rvm.phenotipsRelativesList = new List<ClinicalXPDataConnections.Models.Relative>();
                foreach(var r in ptRels)
                {
                    _rvm.phenotipsRelativesList.Add(new ClinicalXPDataConnections.Models.Relative { DOB = r.DOB, RelTitle = r.RelTitle, 
                        RelForename1 = r.RelForename1, RelSurname = r.RelSurname, DOD = r.DOD, RelSex = r.RelSex, WMFACSID = r.WMFACSID });
                }

                _rvm.relationslist = _relativeData.GetRelationsList();

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportRelatives(int wmfacsid, string firstname, string lastname, DateTime dob, 
            DateTime dod, string sex, string relation)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Relative", "Create", wmfacsid, 0, 0, "", firstname, "", lastname,
                    User.Identity.Name, dob, dod, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-add(SQL)" }); }

                return RedirectToAction("ImportRelatives", "Relative", new { id = wmfacsid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }
    }
}
