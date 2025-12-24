//using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.ViewModels;

namespace ClinicX.Controllers
{
    public class ReviewController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly ReviewVM _rvm;
        private readonly IConfiguration _config;
        private readonly IActivityDataAsync _activityData;        
        private readonly IPatientDataAsync _patientData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IReviewDataAsync _reviewData;
        private readonly IReferralDataAsync _referralData;
        private readonly IAppointmentDataAsync _appointmentData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IAgeCalculator _ageCalculator;

        public ReviewController(IConfiguration config, IPatientDataAsync patientData, IActivityDataAsync activityData, IStaffUserDataAsync staffUserData, IReviewDataAsync reviewData, IReferralDataAsync referralData,
            IAppointmentDataAsync appointmentData, ICRUD crud, IAuditService auditService, IAgeCalculator ageCalculator)
        {
            //_clinContext = context;
            _config = config;
            _rvm = new ReviewVM();
            _patientData = patientData;
            _activityData = activityData;
            _staffUser = staffUserData;
            _reviewData = reviewData;
            _referralData = referralData;
            _appointmentData = appointmentData;
            _crud = crud;
            _audit = auditService;
            _ageCalculator = ageCalculator;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Reviews", "", _ip.GetIPAddress());

                _rvm.reviewList = await _reviewData.GetReviewsList(User.Identity.Name);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        [Authorize]
        public async Task<IActionResult> ReviewsForPatient(int id)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Reviews", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _rvm.reviewList = await _reviewData.GetReviewsListForPatient(id);
                _rvm.patient = await _patientData.GetPatientDetails(id);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Create Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.referral = await _activityData.GetActivityDetails(id);
                _rvm.staffMembers = await _staffUser.GetClinicalStaffList();
                _rvm.patient = await _patientData.GetPatientDetails(_rvm.referral.MPI);
               
                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-add" });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(int mpi, int refID, string pathway, string category, string revDate, string comments, string? recipient)
        {
            try
            {
                if (recipient == null) { recipient = ""; } //set nulls to default values for the SQL
                DateTime reviewDate = new DateTime();

                if (revDate != null)
                {
                    reviewDate = DateTime.Parse(revDate);
                }
                else
                {
                    reviewDate = DateTime.Parse("1/1/1900");
                }


                int success = _crud.CallStoredProcedure("Review", "Create", mpi, refID, 0, pathway, category, recipient, comments, User.Identity.Name,
                     reviewDate);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Review-add(SQL)" }); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-add" });
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.review = await _reviewData.GetReviewDetails(id);
                _rvm.patient = await _patientData.GetPatientDetails(_rvm.review.MPI);
                if (_rvm.review.Planned_Date != null) //show days remaining/overdue
                {
                    _rvm.daysToReview = _ageCalculator.DateDifferenceDay(DateTime.Now, _rvm.review.Planned_Date.GetValueOrDefault());
                    if(_rvm.daysToReview < 0)
                    {
                        _rvm.daysOverdue = _rvm.daysToReview * -1;
                    }
                }

                if (_rvm.review == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string status, string comments, string revDate)
        {
            try
            {
                _rvm.review = await _reviewData.GetReviewDetails(id);

                DateTime reviewDate = new DateTime();

                if (revDate != null)
                {
                    reviewDate = DateTime.Parse(revDate);
                }
                else
                {
                    reviewDate = DateTime.Parse("1/1/1900");
                }
                               
                int success = _crud.CallStoredProcedure("Review", "Edit", id, 0, 0, status, comments, "", "", User.Identity.Name,
                    reviewDate);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Review-edit(SQL)" }); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-edit" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChooseAppt(int id)
        {
            try
            {
                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = user.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinical Notes Choose Appt", id.ToString(), _ip.GetIPAddress());

                _rvm.patient = await _patientData.GetPatientDetails(id);
                _rvm.appointmentList = await _appointmentData.GetAppointmentListByPatient(id);
                _rvm.referralList = await _referralData.GetActiveReferralsListForPatient(id);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}