using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Models;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DiagnosisController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly TestDiseaseVM _dvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IDiseaseDataAsync _diseaseData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit; 

        public DiagnosisController(IConfiguration config, IStaffUserDataAsync staffUserData, IPatientDataAsync patientData, IDiseaseDataAsync diseaseData, ICRUD crud, IAuditService auditService)
        {
            //_clinContext = context;
            _config = config;
            _staffUser = staffUserData;
            _dvm = new TestDiseaseVM();
            _patientData = patientData;
            _diseaseData = diseaseData;
            _crud = crud;
            _audit = auditService;
        }

        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Diagnosis", "", _ip.GetIPAddress());
                _dvm.diagnosisList = await _diseaseData.GetDiseaseListByPatient(id);
                _dvm.patient = await _patientData.GetPatientDetails(id);

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Diagnosis" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddNew(int id, string? searchTerm)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Add New Diagnosis", "ID=" + id.ToString(), _ip.GetIPAddress());
                _dvm.diseaseList = await _diseaseData.GetDiseaseList();
                _dvm.patient = await _patientData.GetPatientDetails(id);
                _dvm.statusList = await _diseaseData.GetStatusList();

                if (searchTerm != null)
                {
                    _dvm.diseaseList = _dvm.diseaseList.Where(d => d.DESCRIPTION.ToUpper().Contains(searchTerm.ToUpper())).ToList();
                    _dvm.searchTerm = searchTerm;
                }

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName= "Diagnosis-new" });
            }
        }

        [HttpPost]
        public IActionResult AddNew(int mpi, string diseaseCode, string status, string comments)
        {
            try
            {
                if (comments == null) { comments = ""; }
                
                int success = _crud.CallStoredProcedure("Diagnosis", "Create", mpi, 0, 0, diseaseCode, status, "", comments, User.Identity.Name);
                //do the update, return 1 if successful and 0 if not

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName= "Diagnosis-new(SQL)" }); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName= "Diagnosis-new" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Diagnosis", "ID=" + id.ToString(), _ip.GetIPAddress());
                _dvm.diagnosis = await _diseaseData.GetDiagnosisDetails(id);               
                _dvm.patient = await _patientData.GetPatientDetails(_dvm.diagnosis.MPI);
                _dvm.statusList = await _diseaseData.GetStatusList();

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName= "Diagnosis-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int diagID, string status, string comments)
        {
            try
            {
                if (diagID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                //we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again
                if (comments == null) { comments = ""; }

                Diagnosis diag = await _diseaseData.GetDiagnosisDetails(diagID);
                int mpi = diag.MPI;
                                
                int success = _crud.CallStoredProcedure("Diagnosis", "Update", diagID, 0, 0, status, "", "", comments, User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Diagnosis-edit(SQL)" }); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Diagnosis-edit" });
            }
        }
    }
}

