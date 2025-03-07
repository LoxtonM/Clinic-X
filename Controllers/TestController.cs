﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using System.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.Data;

namespace ClinicX.Controllers
{
    public class TestController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cXContext;
        private readonly TestDiseaseVM _tvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly ITestData _testData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IAgeCalculator _ageCalculator;

        public TestController(ClinicalContext context, ClinicXContext cXContext, IConfiguration config)
        {
            _clinContext = context;
            _cXContext = cXContext;
            _config = config;
            _tvm = new TestDiseaseVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _testData = new TestData(_clinContext, _cXContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
            _ageCalculator = new AgeCalculator();
        }

        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Tests", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _tvm.patient = _patientData.GetPatientDetails(id);
                _tvm.tests = _testData.GetTestListByPatient(id).OrderBy(t => t.ExpectedDate).ToList();

                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test" });
            }
        }

        [Authorize]
        public async Task<IActionResult> AllOutstandingTests()
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - All Tests", "", _ip.GetIPAddress());

                _tvm.tests = _testData.GetTestListByUser(User.Identity.Name).OrderBy(t => t.ExpectedDate).ToList();

                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "TestAll" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Test", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _tvm.testList = _testData.GetTestList();
                _tvm.patient = _patientData.GetPatientDetails(id);
                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int mpi, string test, string sentTo, DateTime expectedDate)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Test", "Create", mpi, 0, 0, test, sentTo, "", "", User.Identity.Name, expectedDate);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Test-add(SQL)" }); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-add" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Test", "ID=" + id.ToString(), _ip.GetIPAddress());

                _tvm.test = _testData.GetTestDetails(id);
                if (_tvm.test == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                else
                {
                    _tvm.patient = _patientData.GetPatientDetails(_tvm.test.MPI);
                    if (_tvm.test.DATE_REQUESTED != null)
                    {
                        if (_tvm.test.DATE_RECEIVED == null)
                        {
                            _tvm.ageOfTest = _ageCalculator.DateDifferenceDay(_tvm.test.DATE_REQUESTED.GetValueOrDefault(), DateTime.Now);
                        }
                        else
                        {
                            _tvm.ageOfTest = _ageCalculator.DateDifferenceDay(_tvm.test.DATE_REQUESTED.GetValueOrDefault(), _tvm.test.DATE_RECEIVED.GetValueOrDefault());
                        }
                    }
                }
                return View(_tvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int testID, string result, string comments, string receivedDate, string givenDate, int complete)
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
                //int isComplete = 0;
                //if (complete == "Yes")
                //{
                //    isComplete = -1;
                //}

                int success = _crud.CallStoredProcedure("Test", "Update", testID, complete, 0, result, "", "", comments, User.Identity.Name, dateReceived, dateGiven);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Test-edit(SQL)" }); }

                //return RedirectToAction("Index", new { id = mpi });
                return RedirectToAction("AllOutstandingTests");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-edit" });
            }
        }
        
    }
}
