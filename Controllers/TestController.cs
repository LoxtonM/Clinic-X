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
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly TestDiseaseVM _tvm;
        private readonly VMData _vm;
        private readonly CRUD _crud;

        public TestController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _tvm = new TestDiseaseVM();
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                var tests = _vm.GetTestListByPatient(id.GetValueOrDefault());

                return View(tests);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> AllOutstandingTests()
        {
            try
            {
                var tests = _vm.GetTestListByUser(User.Identity.Name);

                return View(tests);
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
                
                _tvm.testList = _vm.GetTestList();
                _tvm.patient = _vm.GetPatientDetails(id);
                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, string test, string sentTo)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Test", "Create", mpi, 0, 0, test, sentTo, "", "", User.Identity.Name);

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
                var test = _vm.GetTestDetails(id);
                if (test == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(test);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int testID, string result, string comments, string receivedDate, string givenDate, Int16 complete)
        {
            try
            {
                if (testID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                DateTime dateReceived = new DateTime();
                DateTime dateGiven = new DateTime();

                if (receivedDate != null)
                {
                    dateReceived = DateTime.Parse(receivedDate);
                }
                else
                {
                    dateReceived = DateTime.Parse("1/1/1900");
                }

                if (givenDate != null)
                {
                    dateGiven = DateTime.Parse(givenDate);
                }
                else
                {
                    dateGiven = DateTime.Parse("1/1/1900");
                    //because we can't have a null date, so we have to convert it to an obviously wrong fixed number and then back again.
                }

                //apparently we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again!
                if (result == null)
                {
                    result = "";
                }

                if (comments == null)
                {
                    comments = "";
                }

                //var patient = await _clinContext.Test.FirstOrDefaultAsync(t => t.TestID == testID);
                int mpi = _vm.GetTestDetails(testID).MPI;
                                                
                int success = _crud.CallStoredProcedure("Test", "Update", testID, complete, 0, result, "", "", comments, User.Identity.Name, dateReceived, dateGiven);

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
