using ClinicX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Models;
using ClinicX.ViewModels;
using ClinicX.Meta;
using System.Net.NetworkInformation;

namespace ClinicX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly DictatedLetterVM lvm;
        private readonly VMData vm;
        private readonly CRUD crud;

        public DictatedLetterController(IConfiguration config, ClinicalContext context)
        {
            _context = context;
            _config = config;
            lvm = new DictatedLetterVM();
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
        public async Task<IActionResult> Edit(int iDID, string sStatus, string sLetterContent, string sLetterContentBold)
        {
            int groop = sLetterContentBold.Length;
            try
            {
                if(sLetterContentBold == null)
                {
                    sLetterContentBold = "";
                }

                crud.CallStoredProcedure("Letter", "Update", iDID, 0, 0, sStatus, "", sLetterContentBold, sLetterContent, User.Identity.Name);

                return RedirectToAction("Edit", new { id = iDID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Approve(int iDID)
        {
            try
            {
                crud.CallStoredProcedure("Letter", "Approve", iDID, 0, 0, "", "", "", "", User.Identity.Name);

                return RedirectToAction("Edit", new { id = iDID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unapprove(int iDID)
        {
            try
            {
                crud.CallStoredProcedure("Letter", "Unapprove", iDID, 0, 0, "", "", "", "", User.Identity.Name);

                return RedirectToAction("Edit", new { id = iDID });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

    }
}
