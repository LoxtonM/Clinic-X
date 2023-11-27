using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Models;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly DictatedLetterVM lvm;
        private readonly VMData vm;

        public DictatedLetterController(ClinicalContext context)
        {
            _context = context;
            lvm = new DictatedLetterVM();
            vm = new VMData(_context);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var users = await _context.StaffMembers.FirstOrDefaultAsync(u => u.EMPLOYEE_NUMBER == User.Identity.Name);

                string strStaffCode = users.STAFF_CODE;

                var letters = from l in _context.DictatedLetters
                              where l.LetterFromCode == strStaffCode && l.MPI != null && l.RefID != null && l.Status != "Printed"
                              orderby l.DateDictated descending
                              select l;

                //letters = letters.OrderByDescending(l => l.DateDictated);

                return View(await letters.ToListAsync());
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {   
                lvm.dictatedLetters = vm.GetDictatedLetterDetails(id);
                lvm.dictatedLettersPatients = vm.GetDictatedLettersPatients(id);
                lvm.dictatedLettersCopies = vm.GetDictatedLettersCopies(id);
                lvm.patients = vm.GetPatients(id);

                return View(lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoTID,RefID,MPI,Status,LetterContent,LetterContentBold")] DictatedLetters dictatedLetters)
        {
            try
            {
                if (id != dictatedLetters.DoTID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {

                    _context.Update(dictatedLetters);
                    //based on Clinical Note creation - review later
                    await _context.SaveChangesAsync();
                    Console.WriteLine("done");

                    return RedirectToAction("Edit", new { id = dictatedLetters.DoTID });
                }
                else
                {
                    return View(dictatedLetters);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        
    }
}
