using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly TestDiseaseVM _dvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IDiseaseData _diseaseData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit; 

        public DiagnosisController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _staffUser = new StaffUserData(_clinContext);
            _dvm = new TestDiseaseVM();            
            _patientData = new PatientData(_clinContext);
            _diseaseData = new DiseaseData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }


        [Authorize]
        public async Task <IActionResult> Index(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Diagnosis");
                _dvm.diagnosisList = _diseaseData.GetDiseaseListByPatient(id);
                _dvm.patient = _patientData.GetPatientDetails(id);

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Add New Diagnosis", id.ToString());
                _dvm.diseaseList = _diseaseData.GetDiseaseList();
                _dvm.patient = _patientData.GetPatientDetails(id);
                _dvm.statusList = _diseaseData.GetStatusList();
                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, string diseaseCode, string status, string comments)
        {
            try
            {
                if (comments == null)
                {
                    comments = "";
                }
                
                int success = _crud.CallStoredProcedure("Diagnosis", "Create", mpi, 0, 0, diseaseCode, status, "", comments, User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Diagnosis", id.ToString());
                _dvm.diagnosis = _diseaseData.GetDiagnosisDetails(id);               
                _dvm.patient = _patientData.GetPatientDetails(_dvm.diagnosis.MPI);
                _dvm.statusList = _diseaseData.GetStatusList();

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
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

                //apparently we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again!
                if (comments == null)
                {
                    comments = "";
                }

                var patient = await _clinContext.Diagnosis.FirstOrDefaultAsync(d => d.ID == diagID);
                int mpi = patient.MPI;
                                
                int success = _crud.CallStoredProcedure("Diagnosis", "Update", diagID, 0, 0, status, "", "", comments, User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}

