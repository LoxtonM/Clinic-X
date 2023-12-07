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
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly CRUD crud;
        private readonly ReviewVM rvm;
        private readonly VMData vm;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            rvm = new ReviewVM();
            vm = new VMData(_context);
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

                var user = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);

                string strStaffCode = user.STAFF_CODE;

                var reviews = from r in _context.Reviews
                              where r.Review_Recipient == strStaffCode && r.Review_Status == "Pending"
                              orderby r.Planned_Date
                              select r;

                return View(await reviews.ToListAsync());
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
                rvm.staffMembers = vm.GetClinicians();
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


                crud.CallStoredProcedure("Review", "Create", iMPI, iRefID, 0, sPathway, sCategory, sRecipient, sComments, User.Identity.Name,
                     dDate);

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
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == id);
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
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == id);

                DateTime dDate = new DateTime();

                if (sDate != null)
                {
                    dDate = DateTime.Parse(sDate);
                }
                else
                {
                    dDate = DateTime.Parse("1/1/1900");
                }
                               
                crud.CallStoredProcedure("Review", "Edit", id, 0, 0, sStatus, sComments, "", "", User.Identity.Name,
                    dDate);


                return RedirectToAction("Index", "Review");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
    }
}
