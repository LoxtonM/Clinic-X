using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using System.Data;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly TestDiseaseVM dvm;
        private readonly VMData _vm;
        private readonly CRUD _crud;

        public DiagnosisController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            dvm = new TestDiseaseVM();
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
        }


        [Authorize]
        public async Task <IActionResult> Index(int id)
        {
            try
            {           
                var diag = _vm.GetDiseaseListByPatient(id);

                return View(diag);
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
                dvm.diseaseList = _vm.GetDiseaseList();
                dvm.patient = _vm.GetPatientDetails(id);
                dvm.statusList = _vm.GetStatusList();
                return View(dvm);
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
                dvm.Diagnosis = _vm.GetDiagnosisDetails(id);
                int iMPI = dvm.Diagnosis.MPI;
                dvm.patient = _vm.GetPatientDetails(iMPI);
                dvm.statusList = _vm.GetStatusList();

                return View(dvm);
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
                int iMPI = patient.MPI;
                                
                int success = _crud.CallStoredProcedure("Diagnosis", "Update", diagID, 0, 0, status, "", "", comments, User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", new { id = iMPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        
    }
}

