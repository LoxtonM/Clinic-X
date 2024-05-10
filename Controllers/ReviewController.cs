using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ICRUD _crud;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rvm = new ReviewVM();
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
            _crud = new CRUD(_config);
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

                _rvm.reviewList = _reviewData.GetReviewsList(User.Identity.Name);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                _rvm.referrals = _activityData.GetActivityDetails(id);
                _rvm.staffMembers = _staffUser.GetClinicalStaffList();
                _rvm.patient = _patientData.GetPatientDetails(_rvm.referrals.MPI);
               
                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int mpi, int refID, string pathway, string category, string revDate, string comments, string? recipient)
        {
            try
            {
                if (recipient == null)
                {
                    recipient = "";
                }
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

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            try
            {
                _rvm.review = _reviewData.GetReviewDetails(id);
                _rvm.patient = _patientData.GetPatientDetails(_rvm.review.MPI);

                if (_rvm.review == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
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

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
    }
}
