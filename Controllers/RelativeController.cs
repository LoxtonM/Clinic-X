using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Controllers
{
    public class RelativeController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly CRUD crud;

        public RelativeController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            crud = new CRUD(_config);
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
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                var rel = await _context.Relatives.FirstOrDefaultAsync(r => r.relsid == id);
                return View(rel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rel = await _context.Relatives.FirstOrDefaultAsync(r => r.relsid == id);
                return View(rel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string sTitle, string sForename1,
            string sForename2, string sSurname, string sRelation, string sDOB, string sDOD,
            int isAffected, string sSex)
        {
            try
            {
                var rel = await _context.Relatives.FirstOrDefaultAsync(r => r.relsid == id);


                DateTime dDOB = new DateTime();
                DateTime dDOD = new DateTime();

                if (sDOB != null)
                {
                    dDOB = DateTime.Parse(sDOB);
                }
                else
                {
                    dDOB = DateTime.Parse("1/1/1900");
                }

                if (sDOD != null)
                {
                    dDOD = DateTime.Parse(sDOD);
                }
                else
                {
                    dDOD = DateTime.Parse("1/1/1900");
                }

                if (sForename2 == null)
                {
                    sForename2 = "";
                }

                crud.CallStoredProcedure("Relative", "Edit", id, isAffected, 0, sTitle, sForename1, sForename2, sSurname,
                        User.Identity.Name, dDOB, dDOD, false, false, 0, 0, 0, sRelation, sSex);

                return RedirectToAction("RelativeDetails", "Relative", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int WMFACSID)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int WMFACSID, string sTitle, string sForename1, 
            string sForename2, string sSurname, string sRelation, string sDOB, string sDOD, 
            int isAffected, string sSex)
        {
            try
            {
                DateTime dDOB = new DateTime();
                DateTime dDOD = new DateTime();

                if (sDOB != null)
                {
                    dDOB = DateTime.Parse(sDOB);
                }
                else
                {
                    dDOB = DateTime.Parse("1/1/1900");
                }

                if (sDOD != null)
                {
                    dDOD = DateTime.Parse(sDOD);
                }
                else
                {
                    dDOD = DateTime.Parse("1/1/1900");
                }

                if (sForename2 == null)
                {
                    sForename2 = "";
                }

                crud.CallStoredProcedure("Relative", "Create", WMFACSID, isAffected, 0, sTitle, sForename1, sForename2, sSurname,
                    User.Identity.Name, dDOB, dDOD, false, false, 0, 0, 0, sRelation, sSex);

                var patient = _context.Patients.FirstOrDefault(p => p.WMFACSID == WMFACSID);

                return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

    }
}
