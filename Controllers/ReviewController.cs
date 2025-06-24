using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.ViewModels;

namespace ClinicX.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ReviewVM _rvm;
        private readonly IConfiguration _config;
        private readonly IActivityData _activityData;        
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IReviewData _reviewData;
        private readonly IReferralData _referralData;
        private readonly IAppointmentData _appointmentData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IAgeCalculator _ageCalculator;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rvm = new ReviewVM();
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _appointmentData = new AppointmentData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
            _ageCalculator = new AgeCalculator();
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

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Reviews", "", _ip.GetIPAddress());

                _rvm.reviewList = _reviewData.GetReviewsList(User.Identity.Name);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review" });
            }
        }

        public async Task<IActionResult> ReviewsForPatient(int id)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Reviews", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _rvm.reviewList = _reviewData.GetReviewsListForPatient(id);
                _rvm.patient = _patientData.GetPatientDetails(id);

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Create Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.referral = _activityData.GetActivityDetails(id);
                _rvm.staffMembers = _staffUser.GetClinicalStaffList();
                _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);
               
                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Review-add" });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int mpi, int refID, string pathway, string category, string revDate, string comments, string? recipient)
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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Review", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.review = _reviewData.GetReviewDetails(id);
                _rvm.patient = _patientData.GetPatientDetails(_rvm.review.MPI);
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
                _rvm.review = _reviewData.GetReviewDetails(id);

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
        public async Task<IActionResult> ChooseAppt(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Clinical Notes Choose Appt", id.ToString(), _ip.GetIPAddress());

                _rvm.patient = _patientData.GetPatientDetails(id);
                _rvm.appointmentList = _appointmentData.GetAppointmentListByPatient(id);
                _rvm.referralList = _referralData.GetActiveReferralsListForPatient(id);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
