using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using System.Data;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
//using ClinicX.Data;
using ClinicX.Models;
using Microsoft.Office.Interop.Outlook;

namespace ClinicX.Controllers
{
    public class TestController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        //private readonly DocumentContext _documentContext;
        private readonly TestDiseaseVM _tvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly ITestData _testData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IAgeCalculator _ageCalculator;
        private readonly IBloodFormData _bloodFormData;
        private readonly ISampleData _sampleData;
        private readonly IConstantsData _constantsData;
        private readonly IReferralData _refData;
        private readonly BloodFormController _bfc;

        public TestController(IConfiguration config, IStaffUserData staffUserData, IPatientData patientData, ITestData testData, ICRUD crud, IAuditService audit, IAgeCalculator ageCalculator, 
            IBloodFormData bloodFormData, ISampleData sampleData, IConstantsData constantsData, IReferralData referralData, BloodFormController bloodFormController)
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            //_documentContext = documentContext;
            _config = config;
            _tvm = new TestDiseaseVM();
            _staffUser = staffUserData;
            //_staffUser = new StaffUserData(_clinContext);
            //_patientData = new PatientData(_clinContext);
            _patientData = patientData;
            //_testData = new TestData(_clinContext, _cXContext);
            _testData = testData;
            //_crud = new CRUD(_config);
            _crud = crud;
            //_audit = new AuditService(_config);
            _audit = audit;
            //_ageCalculator = new AgeCalculator();
            _ageCalculator = ageCalculator;
            //_bloodFormData = new BloodFormData(_cXContext);
            _bloodFormData = bloodFormData;
            //_sampleData = new SampleData(_cXContext);
            _sampleData = sampleData;
            //_constantsData = new ConstantsData(_documentContext);
            _constantsData = constantsData;
            //_refData = new ReferralData(_clinContext);
            _refData = referralData;
            //BloodFormController bfc = new BloodFormController(_clinContext, _cXContext);
            _bfc = bloodFormController;
        }

        [Authorize]
        public IActionResult Index(int id)
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
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test" });
            }
        }

        [Authorize]
        public IActionResult AllOutstandingTests()
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - All Tests", "", _ip.GetIPAddress());

                _tvm.tests = _testData.GetTestListByUser(User.Identity.Name).OrderBy(t => t.ExpectedDate).ToList();

                return View(_tvm);
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "TestAll" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult AddNew(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - New Test", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _tvm.testList = _testData.GetTestList();
                _tvm.patient = _patientData.GetPatientDetails(id);
                _tvm.referralList = _refData.GetActiveReferralsListForPatient(id).OrderByDescending(r => r.RefDate).ToList();

                return View(_tvm);
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-add" });
            }
        }

        [HttpPost]
        public IActionResult AddNew(int mpi, string test, string sentTo, DateTime expectedDate, int refID)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Test", "Create", mpi, refID, 0, test, sentTo, "", "", User.Identity.Name, expectedDate);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Test-add(SQL)" }); }

                return RedirectToAction("Index", new { id = mpi });
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-add" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
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
                _tvm.bloodFormList = _bloodFormData.GetBloodFormList(id);
                _tvm.edmsLink = _constantsData.GetConstant("GEMRLink", 1) + _tvm.patient.DCTM_Folder_ID + "/cg_view_pedigree_patie";

                return View(_tvm);
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-edit" });
            }
        }

        [HttpPost]
        public IActionResult Edit(int testID, string result, string comments, string receivedDate, string givenDate, int complete)
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
                    dateReceived = DateTime.Parse("1900-01-01");
                }

                if (givenDate != null)
                {
                    dateGiven = DateTime.Parse(givenDate);
                }
                else
                {
                    dateGiven = DateTime.Parse("1900-01-01");
                    //because we can't have a null date, so we have to convert it to an obviously wrong fixed number and then back again.
                }

                //we simply can't send a null parameter to the SQL, so we have to convert it to an empty string and then back again!
                if (result == null) { result = ""; }

                if (comments == null) { comments = ""; }

                int mpi = _testData.GetTestDetails(testID).MPI;

                int success = _crud.CallStoredProcedure("Test", "Update", testID, complete, 0, result, "", "", comments, User.Identity.Name, dateReceived, dateGiven);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Test-edit(SQL)" }); }

                return RedirectToAction("AllOutstandingTests");
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Test-edit" });
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult NewBloodForm(int testID)
        {
            _tvm.test = _testData.GetTestDetails(testID);
            _tvm.patient = _patientData.GetPatientDetails(_tvm.test.MPI);            
            _tvm.sampleTypes = _sampleData.GetSampleTypeList();
            _tvm.sampleRequirementList = _sampleData.GetSampleRequirementsList();

            return View(_tvm);
        }

        [HttpPost]
        public IActionResult NewBloodForm(int testID, int iSampleRequirements, string? clinicalDetails, string? testingRequirements, string sampleType, string relativeDetails,
            DateTime? nextAppDate, DateTime? relDOB, bool isNHS, bool isUrgent, string? relname, string sampleDetails, bool isInpatient, string? relNumber,
            bool isPrenatal, bool isPresymptomatic, bool isDiagnostic, bool isCarrier, string? prenatalType, string? prenatalRisk, int? gestation)
        {
            int success = _crud.CallPatientBloodFormCRUD("Create", testID, iSampleRequirements, gestation.GetValueOrDefault(), 0, 0, 0, 0, clinicalDetails, testingRequirements, 
                sampleType, relativeDetails, User.Identity.Name, prenatalType, prenatalRisk, relname, relNumber, "", nextAppDate, relDOB, isNHS, isUrgent, isInpatient, 
                isPrenatal, isPresymptomatic, isDiagnostic, isCarrier);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Test-edit(SQL)" }); }

            int iBloodFormID = _bloodFormData.GetBloodFormList(testID).OrderByDescending(f => f.BloodFormID).First().BloodFormID;


            return RedirectToAction("BloodFormEdit", new { bloodFormID = iBloodFormID });
        }

        [HttpGet]
        [Authorize]
        public IActionResult BloodFormEdit(int bloodFormID) //save data to use in the blood form preview
        {
            _tvm.bloodForm = _bloodFormData.GetBloodFormDetails(bloodFormID);
            _tvm.test = _testData.GetTestDetails(_tvm.bloodForm.TestID);
            _tvm.patient = _patientData.GetPatientDetails(_tvm.test.MPI);
            
            _tvm.sampleTypes = _sampleData.GetSampleTypeList();
            _tvm.sampleRequirementList = _sampleData.GetSampleRequirementsList();

            return View(_tvm);
        }

        [HttpPost]
        public IActionResult BloodFormEdit(int bloodFormID, string sampleRequirements, string? clinicalDetails, string? testingRequirements, string sampleType, string relativeDetails,
            DateTime? nextAppDate, DateTime? relDOB, bool isNHS, bool isUrgent, string? relname, string sampleDetails, bool isInpatient, string? relNumber,
            bool isPrenatal, bool isPresymptomatic, bool isDiagnostic, bool isCarrier, string? prenatalType, string? prenatalRisk, int? gestation)
        {
            _tvm.bloodForm = _bloodFormData.GetBloodFormDetails(bloodFormID);
            _tvm.test = _testData.GetTestDetails(_tvm.bloodForm.TestID);
            _tvm.patient = _patientData.GetPatientDetails(_tvm.test.MPI);

            _tvm.sampleTypes = _sampleData.GetSampleTypeList();
            _tvm.sampleRequirementList = _sampleData.GetSampleRequirementsList();

            int iSuccess = _crud.CallPatientBloodFormCRUD("Edit", bloodFormID, 0, gestation.GetValueOrDefault(), 0, 0, 0, 0, clinicalDetails, testingRequirements,
                sampleType, relativeDetails, User.Identity.Name, prenatalType, prenatalRisk, relname, relNumber, sampleRequirements, nextAppDate, relDOB, isNHS, isUrgent, isInpatient,
                isPrenatal, isPresymptomatic, isDiagnostic, isCarrier);

            return RedirectToAction("BloodFormEdit", new { bloodFormID = bloodFormID });
        }

        [Authorize]
        public IActionResult DoBloodForm(int bloodFormID, string? altPatName, bool? isPreview = false) //create the blood form itself
        {
            //BloodFormData bfData = new BloodFormData(_cXContext);
            BloodForm bf = _bloodFormData.GetBloodFormDetails(bloodFormID);
            int testID = bf.TestID;

            

            _bfc.CreateBloodForm(bloodFormID, User.Identity.Name, altPatName, isPreview);

            //return RedirectToAction("Edit", new { id = testID});
            return File($"~/StandardLetterPreviews/bloodform-{User.Identity.Name}.pdf", "Application/PDF");
        }
    }
}
