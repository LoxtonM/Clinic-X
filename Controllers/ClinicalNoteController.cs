using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly PatientData _patientData;
        private readonly ActivityData _activityData;
        private readonly ClinicalNoteData _clinicalNoteData;
        private readonly ClinicalNoteVM _cvm;
        private readonly ClinicData _clinicData;
        private readonly MiscData _misc;
        private readonly CRUD _crud;

        public ClinicalNoteController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;            
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _clinicalNoteData = new ClinicalNoteData(_clinContext);
            _clinicData = new ClinicData(_clinContext);
            _crud = new CRUD(_config);
            _cvm = new ClinicalNoteVM();
            _misc = new MiscData(_config);
        }       
        
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            try
            {                                        
                _cvm.clinicalNotesList = _clinicalNoteData.GetClinicalNoteList(id);
                _cvm.patient = _patientData.GetPatientDetails(id);
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
                _cvm.activityItem = _activityData.GetActivityDetails(id);
                _cvm.noteTypeList = _clinicalNoteData.GetNoteTypesList();

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
                _cvm.clinicalNote = _clinicalNoteData.GetClinicalNoteDetails(id);
                _cvm.patient = _patientData.GetPatientDetails(_cvm.clinicalNote.MPI.GetValueOrDefault());

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
                _cvm.patient = _patientData.GetPatientDetails(id);
                _cvm.Clinics = _clinicData.GetClinicByPatientsList(id);

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
