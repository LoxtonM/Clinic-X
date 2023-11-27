using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ClinicX.Meta;
using System.Security.Cryptography.X509Certificates;

namespace ClinicX.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly CRUD crud;

        public ReviewController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
