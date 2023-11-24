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
    public class TestController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;

        public TestController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                var tests = from t in _context.Test
                            where t.MPI.Equals(id)
                            select t;

                return View(await tests.ToListAsync());
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> AllOutstandingTests()
        {
            try
            {
                var user = await _context.StaffMembers.FirstOrDefaultAsync(s => s.EMPLOYEE_NUMBER == User.Identity.Name);
                string sStaffCode = user.STAFF_CODE;

                var tests = from t in _context.Test
                            where t.ORDEREDBY.Equals(sStaffCode) & t.COMPLETE == "No"
                            select t;

                return View(await tests.ToListAsync());
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                TestDiseaseVM tvm = new TestDiseaseVM();
                VMData vm = new VMData(_context);
                tvm.testList = vm.GetTests();
                tvm.patient = vm.GetPatientDetails(id);
                return View(tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int iMPI, string sTest, string sSentTo)
        {
            try
            {
                CRUD crud = new CRUD(_config);
                crud.CallStoredProcedure("Test", "Create", iMPI, 0, 0, sTest, sSentTo, "", "", User.Identity.Name);

                return RedirectToAction("Index", new { id = iMPI });
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
                var test = await _context.Test.FirstOrDefaultAsync(t => t.TestID == id);
                if (test == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(test);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int iTestID, string sResult, string sComments, string sReceived, string sGiven, Int16 iComplete)
        {
            try
            {
                if (iTestID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                DateTime dReceived = new DateTime();
                DateTime dGiven = new DateTime();

                if (sReceived != null)
                {
                    dReceived = DateTime.Parse(sReceived);
                }
                else
                {
                    dReceived = DateTime.Parse("1/1/1900");
                }

                if (sGiven != null)
                {
                    dGiven = DateTime.Parse(sGiven);
                }
                else
                {
                    dGiven = DateTime.Parse("1/1/1900");
                    //because we can't have a null date, so we have to convert it to an obviously wrong fixed number and then back again.
                }

                //apparently we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again!
                if (sResult == null)
                {
                    sResult = "";
                }

                if (sComments == null)
                {
                    sComments = "";
                }

                var patient = await _context.Test.FirstOrDefaultAsync(t => t.TestID == iTestID);
                int iMPI = patient.MPI;

                CRUD crud = new CRUD(_config);
                crud.CallStoredProcedure("Test", "Update", iTestID, iComplete, 0, sResult, "", "", sComments, User.Identity.Name, dReceived, dGiven);

                return RedirectToAction("Index", new { id = iMPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
        
    }
}
