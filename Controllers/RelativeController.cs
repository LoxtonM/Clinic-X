using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ClinicX.ViewModels;

namespace ClinicX.Controllers
{
    public class RelativeController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly CRUD _crud;
        private readonly VMData _vm;
        private readonly RelativeDiagnosisVM _rdvm;

        public RelativeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _crud = new CRUD(_config);
            _vm = new VMData(_clinContext);
            _rdvm = new RelativeDiagnosisVM();
        }

        [Authorize]
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                _rdvm.relativeDetails = _vm.GetRelativeDetails(id);
                _rdvm.MPI = _vm.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID).MPI;
                
                return View(_rdvm);
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
                _rdvm.relativeDetails = _vm.GetRelativeDetails(id);
                _rdvm.relationList = _vm.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _vm.GetGenderList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string forename1,
            string forename2, string surname, string relation, string dob, string dod,
            int isAffected, string sex)
        {
            try
            {
                _rdvm.relativeDetails = _vm.GetRelativeDetails(id);

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

                if (title == null)
                {
                    title = "";
                }

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Edit", id, isAffected, 0, title, forename1, forename2, surname,
                        User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("RelativeDetails", "Relative", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int wmfacsid)
        {
            try
            {
                _rdvm.WMFACSID = wmfacsid;
                _rdvm.MPI = _vm.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                _rdvm.relationList = _vm.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rdvm.genderList = _vm.GetGenderList();
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int wmfacsid, string title, string forename1, 
            string forename2, string surname, string relation, string sDOB, string sDOD, 
            int isAffected, string sex)
        {
            try
            {
                _rdvm.MPI = _vm.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
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

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Create", wmfacsid, isAffected, 0, title, forename1, forename2, surname,
                    User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                var patient = _vm.GetPatientDetailsByWMFACSID(wmfacsid);

                return RedirectToAction("PatientDetails", "Patient", new { id = _rdvm.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
