using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicX.Meta;
using ClinicX.Data;
using ClinicalXPDataConnections.Models;

namespace ClinicX.Controllers
{
    public class HPOController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cXContext;
        private readonly DocumentContext _docContext;
        private readonly HPOVM _hpo;
        private readonly IStaffUserData _staffUser;
        private readonly IConfiguration _config;        
        private readonly IHPOCodeData _hpoData;
        private readonly IClinicalNoteData _clinicaNoteData;        
        private readonly IMiscData _misc;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly APIController _api;

        public HPOController(ClinicalContext context, ClinicXContext cXContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = context;
            _cXContext = cXContext;
            _docContext = docContext;
            _config = config;
            _staffUser = new StaffUserData(_clinContext);
            _hpo = new HPOVM();
            _hpoData = new HPOCodeData(_clinContext);
            _clinicaNoteData = new ClinicalNoteData(_cXContext);
            _crud = new CRUD(_config);
            _misc = new MiscData(_config);
            _audit = new AuditService(_config);
            _api = new APIController(_clinContext, _docContext, _config);
        }

        [HttpGet]
        public async Task<IActionResult> HPOTerm(int id, string? searchTerm)
        {
            try
            {
                _hpo.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _hpo.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - HPO", "ID=" + id.ToString(), _ip.GetIPAddress());
                
                _hpo.clinicalNote = _clinicaNoteData.GetClinicalNoteDetails(id);
                _hpo.hpoTermDetails = _hpoData.GetExistingHPOTermsList(id);
                //_hpo.hpoTerms = _hpoData.GetHPOTermsList();
                _hpo.hpoExtractedTerms = _hpoData.GetExtractedTermsList(id, _config);

                if(searchTerm != null) 
                {
                    _hpo.hpoTerms = await _api.GetHPOCodes(searchTerm);

                    _hpo.searchTerm = searchTerm;
                }

                return View(_hpo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "HPO" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddHPOTerm(int noteID, string termCode)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - HPO", "NoteID=" + noteID.ToString(), _ip.GetIPAddress());
                //check if code exists, add it if it doesn't
                if (_hpoData.GetHPOTermByTermCode(termCode) == null)
                {                    
                    HPOTerm term = await _api.GetHPODataByTermCode(termCode);
                    _hpoData.AddHPOTermToDatabase(termCode, term.Term, staffCode, _config);

                    //and add its synonyms as well
                    HPOTerm termAdded = _hpoData.GetHPOTermByTermCode(termCode);
                    int hpoTermID = termAdded.ID;

                    List<string> synonymsToAdd = new List<string>();
                    synonymsToAdd = await _api.GetHPOSynonymsByTermCode(termCode);

                    foreach (var item in synonymsToAdd)
                    {
                        _hpoData.AddHPOSynonymToDatabase(hpoTermID, item, staffCode, _config);
                    }
                }

                int success = _crud.CallStoredProcedure("Clinical Note", "Add HPO Term", noteID, 0, 0, termCode, "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "HPO-add(SQL)" }); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "HPO-add" });
            }
        }
                
        [HttpPost]
        public async Task<IActionResult> AddHPOTermFromText(int termID, int noteID)
        {
            try
            {
                string termCode = _hpoData.GetHPOTermByID(termID).TermCode;

                int success = _crud.CallStoredProcedure("Clinical Note", "Add HPO Term", noteID, 0, 0, termCode, "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "HPO-addFromText(SQL)" }); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "HPO-addFromText" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHPOTermFromNote(int id)
        {
            try
            {
                //int noteID = 0;
                int noteID = _misc.GetNoteIDFromHPOTerm(id);

                int success = _crud.CallStoredProcedure("Clinical Note", "Delete HPO Term", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "HPO-delete(SQL)" }); }

                return RedirectToAction("HPOTerm", new { id = noteID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "HPO-delete" });
            }
        }  
    }
}
