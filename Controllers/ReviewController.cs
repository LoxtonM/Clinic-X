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
        private readonly CRUD crud;
        private readonly ReviewVM rvm;
        private readonly VMData vm;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            rvm = new ReviewVM();
            vm = new VMData(_clinContext);
            crud = new CRUD(_config);
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

                var reviews = vm.GetReviewsList(User.Identity.Name);

                return View(reviews);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {
                rvm.referrals = vm.GetActivityDetails(id);
                rvm.staffMembers = vm.GetClinicalStaffList();
                int impi = rvm.referrals.MPI;
                rvm.patient = vm.GetPatientDetails(impi);
               
                return View(rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int iMPI, int iRefID, string sPathway, string sCategory, string sDate, string sComments, string? sRecipient)
        {
            try
            {
                if (sRecipient == null)
                {
                    sRecipient = "";
                }
                DateTime dDate = new DateTime();

                if (sDate != null)
                {
                    dDate = DateTime.Parse(sDate);
                }
                else
                {
                    dDate = DateTime.Parse("1/1/1900");
                }


                int iSuccess = crud.CallStoredProcedure("Review", "Create", iMPI, iRefID, 0, sPathway, sCategory, sRecipient, sComments, User.Identity.Name,
                     dDate);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit (int id)
        {
            try
            {
                var review = vm.GetReviewDetails(id);
                
                
                if (review == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                return View(review);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string sStatus, string sComments, string sDate)
        {
            try
            {
                var review = vm.GetReviewDetails(id);

                DateTime dDate = new DateTime();

                if (sDate != null)
                {
                    dDate = DateTime.Parse(sDate);
                }
                else
                {
                    dDate = DateTime.Parse("1/1/1900");
                }
                               
                int iSuccess = crud.CallStoredProcedure("Review", "Edit", id, 0, 0, sStatus, sComments, "", "", User.Identity.Name,
                    dDate);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
