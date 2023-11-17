using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClinicX.Data;
using ClinicX.ViewModels;
using ClinicX.Models;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class ClinicalNoteController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;

        public ClinicalNoteController(ClinicalContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }       
        
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {

            var notes = from c in _context.ClinicalNotes
                        where c.MPI.Equals(id) //& c.ClinicalNote != null
                        orderby c.CreatedDate, c.CreatedTime descending
                        select c;

            int count = notes.Count();

            return View(await notes.ToListAsync());            
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {                        
            ClinicVM cvm = new ClinicVM();
            VMData vm = new VMData(_context);
            cvm.activityItem = vm.GetClinicDetails(id);
            cvm.noteTypeList = vm.GetNoteTypes();

            return View(cvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("MPI, RefID, CreatedBy, NoteType, ClinicalNote, CN_DCTM_sts")] NoteItems noteItems)
        public async Task<IActionResult> Create(int iMPI, int iRefID, string sNoteType, string sClinicalNote)
        {
            //if (ModelState.IsValid)
            //{                 
            int iNoteID;
                          
            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("Clinical Note", "Create", iMPI, iRefID, 0, sNoteType, "", "", 
                sClinicalNote, User.Identity.Name);

            iNoteID = GetClinicalNoteID(iRefID);

            return RedirectToAction("Edit", new {id = iNoteID });
            //}
            //return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var notes = await _context.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);
            return View(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(int iNoteID, string sClinicalNote)
        {
            if(iNoteID == null)
            {
                return RedirectToAction("NotFound", "WIP");
            }

            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("Clinical Note", "Update", iNoteID, 0, 0, "", "", "", sClinicalNote, User.Identity.Name);

            return RedirectToAction("Edit", new { id = iNoteID });                    
        }

        public async Task<IActionResult> ChooseAppt(int id)
        {
            var appts = from c in _context.Clinics
                        where c.MPI.Equals(id)
                        orderby c.BOOKED_DATE descending
                        select c;

            //appts = appts.OrderByDescending(c => c.BOOKED_DATE);

            return View(await appts.ToListAsync());
        }

        public async Task<IActionResult> Finalise(int id)
        {
            CRUD crud = new CRUD(_config);
            crud.CallStoredProcedure("Clinical Note", "Finalise", id, 0, 0, "", "", "", "", User.Identity.Name);

            var note = await _context.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);

            return RedirectToAction("Index", new { id = note.MPI });
        }

        public int GetClinicalNoteID(int iRefID)
        {
            int iNoteID;
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd2 = new SqlCommand("select top 1 clinicalnoteid from clinicalnotes " +
                    "where refid = " + iRefID.ToString() + " order by createddate desc, createdtime desc", conn);

            iNoteID = (int)(cmd2.ExecuteScalar());

            conn.Close();
            return iNoteID;
        }

    }
}
