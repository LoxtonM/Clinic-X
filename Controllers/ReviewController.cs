using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ClinicX.Meta;
using System.Security.Cryptography.X509Certificates;
using ClinicX.ViewModels;
using System.Net.NetworkInformation;

namespace ClinicX.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly CRUD _crud;
        private readonly ReviewVM _rvm;
        private readonly VMData _vm;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _rvm = new ReviewVM();
            _vm = new VMData(_clinContext);
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

                var reviews = _vm.GetReviewsList(User.Identity.Name);

                return View(reviews);
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
                _rvm.referrals = _vm.GetActivityDetails(id);
                _rvm.staffMembers = _vm.GetClinicalStaffList();
                int impi = _rvm.referrals.MPI;
                _rvm.patient = _vm.GetPatientDetails(impi);
               
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
                var review = _vm.GetReviewDetails(id);
                
                
                if (review == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(review);
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
                var review = _vm.GetReviewDetails(id);

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
