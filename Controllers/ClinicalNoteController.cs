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
        private readonly VMData _vm;
        private readonly CRUD _crud;
        //private readonly ClinicVM _cvm;
        private readonly ClinicalNoteVM _cvm;
        private readonly MiscData _misc;

        public ClinicalNoteController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
            _cvm = new ClinicalNoteVM();
            _misc = new MiscData(_config);
        }       
        
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {                                        
                _cvm.clinicalNotesList = _vm.GetClinicalNoteList(id);
                _cvm.patient = _vm.GetPatientDetails(id);
                _cvm.noteCount = _cvm.clinicalNotesList.Count();                

                return View(_cvm);
            }
            catch(Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            try
            {                
                _cvm.activityItem = _vm.GetActivityDetails(id);
                _cvm.noteTypeList = _vm.GetNoteTypesList();

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int mpi, int refID, string noteType, string clinicalNote)
        {
            try
            {                                
                int noteID;

                int success = _crud.CallStoredProcedure("Clinical Note", "Create", mpi, refID, 0, noteType, "", "",
                    clinicalNote, User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                noteID = _misc.GetClinicalNoteID(refID);

                return RedirectToAction("Edit", new { id = noteID });                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {                
                _cvm.clinicalNote = _vm.GetClinicalNoteDetails(id);
                _cvm.patient = _vm.GetPatientDetails(_cvm.clinicalNote.MPI.GetValueOrDefault());

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(int noteID, string clinicalNote)
        {
            try
            {
                if (noteID == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                int success = _crud.CallStoredProcedure("Clinical Note", "Update", noteID, 0, 0, 
                    "", "", "", clinicalNote, User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("Edit", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChooseAppt(int id)
        {
            try
            {
                //var appts = _vm.GetClinicByPatientsList(id);
                _cvm.patient = _vm.GetPatientDetails(id);
                _cvm.Clinics = _vm.GetClinicByPatientsList(id);

                return View(_cvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Finalise(int id)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Clinical Note", "Finalise", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                var note = await _clinContext.NoteItems.FirstOrDefaultAsync(c => c.ClinicalNoteID == id);

                return RedirectToAction("Index", new { id = note.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }        

    }
}
