using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.Data.SqlClient;
using ClinicX.Models;
using System.Data;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;

        public DiagnosisController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        [Authorize]
        public async Task <IActionResult> Index(int id)
        {
            var diag = from d in _context.Diagnosis
                          where d.MPI.Equals(id)
                          select d;

            return View(await diag.ToListAsync());
            
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            TestDiseaseVM dvm = new TestDiseaseVM();
            VMData vm = new VMData(_context);
            dvm.diseaseList = vm.GetDisease();
            dvm.patient = vm.GetPatientDetails(id);
            dvm.statusList = vm.GetStatusList();
            return View(dvm);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int iMPI, string sDiseaseCode, string sStatus, string sComments)
        {
            if (sComments == null)
            {
                sComments = "";
            }
            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("Diagnosis", "Create", iMPI, 0, 0, sDiseaseCode, sStatus, "", sComments, User.Identity.Name);
            
            return RedirectToAction("Index", new { id = iMPI});
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            TestDiseaseVM dvm = new TestDiseaseVM();
            VMData vm = new VMData(_context);
            dvm.Diagnosis = vm.GetDiagnosisDetails(id);
            int iMPI = dvm.Diagnosis.MPI;
            dvm.patient = vm.GetPatientDetails(iMPI);
            dvm.statusList = vm.GetStatusList();
            
            return View(dvm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int iDiagID, string sStatus, string sComments, string sUpdatedBy)
        {
            if (iDiagID == null)
            {
                return RedirectToAction("NotFound", "WIP");
            }
                                   
            //apparently we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again!
            if (sComments == null)
            {
                sComments = "";
            }
            
            var patient = await _context.Diagnosis.FirstOrDefaultAsync(d => d.ID == iDiagID);
            int iMPI = patient.MPI;

            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("Diagnosis", "Update", iDiagID, 0, 0, sStatus, "", "", sComments, User.Identity.Name);

            return RedirectToAction("Index", new { id = iMPI });
        }

        
    }
}

