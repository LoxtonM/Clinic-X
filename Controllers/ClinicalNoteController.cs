using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClinicX.Data;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class ClinicalNoteController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly VMData vm;
        private readonly CRUD crud;
        private readonly ClinicVM cvm;
        private readonly MiscData misc;

        public ClinicalNoteController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            vm = new VMData(_clinContext);
            crud = new CRUD(_config);
            cvm = new ClinicVM();
            misc = new MiscData(_config);
        }       
        
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                /*var notes = from c in _clinContext.ClinicalNotes
                            where c.MPI.Equals(id)
                            orderby c.CreatedDate, c.CreatedTime descending
                            select c;*/

                var notes = vm.GetClinicalNoteList(id);
                                

                int count = notes.Count();

                return View(notes);
            }
            catch(Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {                
                cvm.activityItem = vm.GetActivityDetails(id);
                cvm.noteTypeList = vm.GetNoteTypesList();

                return View(cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int iMPI, int iRefID, string sNoteType, string sClinicalNote)
        {
            try
            {                                
                int iNoteID;

                int iSuccess = crud.CallStoredProcedure("Clinical Note", "Create", iMPI, iRefID, 0, sNoteType, "", "",
                    sClinicalNote, User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                iNoteID = misc.GetClinicalNoteID(iRefID);

                return RedirectToAction("Edit", new { id = iNoteID });                
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
                //var notes = await _clinContext.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);
                var note = vm.GetClinicalNoteDetails(id);
                
                return View(note);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(int iNoteID, string sClinicalNote)
        {
            try
            {
                if (iNoteID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                int iSuccess = crud.CallStoredProcedure("Clinical Note", "Update", iNoteID, 0, 0, 
                    "", "", "", sClinicalNote, User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = iNoteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChooseAppt(int id)
        {
            try
            {   
                var appts = vm.GetClinicByPatientsList(id);

                return View(appts);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }

        public async Task<IActionResult> Finalise(int id)
        {
            try
            {
                int iSuccess = crud.CallStoredProcedure("Clinical Note", "Finalise", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (iSuccess == 0) { return RedirectToAction("Index", "WIP"); }

                var note = await _clinContext.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);

                return RedirectToAction("Index", new { id = note.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { sError = ex.Message });
            }
        }        

    }
}
