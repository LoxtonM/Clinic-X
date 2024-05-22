using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using ClinicX.ViewModels;
using System.Data;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class TestController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly TestDiseaseVM _tvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly ITestData _testData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public TestController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _tvm = new TestDiseaseVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _testData = new TestData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Tests", id.ToString());

                _tvm.patient = _patientData.GetPatientDetails(id);
                _tvm.tests = _testData.GetTestListByPatient(id).OrderBy(t => t.ExpectedDate).ToList();

                return View(_tvm);
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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - All Tests");

                _tvm.tests = _testData.GetTestListByUser(User.Identity.Name).OrderBy(t => t.ExpectedDate).ToList();

                return View(_tvm);
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
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Test", id.ToString());

                _tvm.testList = _testData.GetTestList();
                _tvm.patient = _patientData.GetPatientDetails(id);
                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, string test, string sentTo, DateTime expectedDate)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Test", "Create", mpi, 0, 0, test, sentTo, "", "", User.Identity.Name, expectedDate);

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
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Test", id.ToString());

                _tvm.test = _testData.GetTestDetails(id);
                _tvm.patient = _patientData.GetPatientDetails(_tvm.test.MPI);
                if (_tvm.test == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_tvm);
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
                int mpi = _testData.GetTestDetails(testID).MPI;
                                                
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
