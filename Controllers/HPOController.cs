using Microsoft.AspNetCore.Mvc;
using ClinicX.Data;
using ClinicX.ViewModels;
using ClinicX.Meta;

namespace ClinicX.Controllers
{
    public class HPOController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly HPOVM _hpo;
        private readonly VMData _vm;
        private readonly CRUD _crud;
        private readonly MiscData _misc;

        public HPOController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _hpo = new HPOVM();
            _vm = new VMData(_clinContext);
            _crud = new CRUD(_config);
            _misc = new MiscData(_config);
        }

        [HttpGet]
        public async Task<IActionResult> HPOTerm(int id, string? searchTerm)
        {
            try
            {                
                _hpo.clinicalNote = _vm.GetClinicalNoteDetails(id);
                _hpo.hpoTermDetails = _vm.GetExistingHPOTermsList(id);
                _hpo.hpoTerms = _vm.GetHPOTermsList();
                _hpo.hpoExtractVM = _vm.GetExtractedTermsList(id, _config);

                if(searchTerm != null) 
                { 
                    _hpo.hpoTerms = _hpo.hpoTerms.Where(t => t.Term.Contains(searchTerm)).ToList();
                    _hpo.searchTerm = searchTerm;
                }

                return View(_hpo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddHPOTerm(int noteID, int termID)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Clinical Note", "Add HPO Term", noteID, termID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }
                
        [HttpPost]
        public async Task<IActionResult> AddHPOTermFromText(int termID, int noteID)
        {
            try
            {                
                int success = _crud.CallStoredProcedure("Clinical Note", "Add HPO Term", noteID, termID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHPOTermFromNote(int id)
        {
            try
            {
                //int iNoteID = 0;
                int noteID = _misc.GetNoteIDFromHPOTerm(id);

                int success = _crud.CallStoredProcedure("Clinical Note", "Delete HPO Term", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("Index", "WIP"); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message });
            }
        }  
    }
}
