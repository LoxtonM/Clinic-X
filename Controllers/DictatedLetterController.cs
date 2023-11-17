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

        public DictatedLetterController(ClinicalContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //var letter = await _context.DictatedLetters.FirstOrDefaultAsync(c => c.DoTID == id);

            //return View(letter);
            DictatedLetterVM lvm = new DictatedLetterVM();
            VMData vm = new VMData(_context);
            lvm.dictatedLetters = vm.GetDictatedLetterDetails(id);
            lvm.dictatedLettersPatients = vm.GetDictatedLettersPatients(id);
            lvm.dictatedLettersCopies = vm.GetDictatedLettersCopies(id);
            lvm.patients = vm.GetPatients(id);

            return View(lvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoTID,RefID,MPI,Status,LetterContent,LetterContentBold")] DictatedLetters dictatedLetters)
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
    }
}
