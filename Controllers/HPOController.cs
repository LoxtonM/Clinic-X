using APIControllers.Controllers;
//using APIControllers.Data;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
//using ClinicX.Data;
using ClinicX.Meta;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class HPOController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly ClinicXContext _cXContext;
        //private readonly APIContext _apiContext;
        private readonly HPOVM _hpo;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IConfiguration _config;        
        private readonly IHPOCodeDataAsync _hpoData;
        private readonly IClinicalNoteDataAsync _clinicaNoteData;        
        private readonly IMiscData _misc;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly IApiController _api;

        public HPOController(IConfiguration config, IStaffUserDataAsync staffUserData, IHPOCodeDataAsync hPOCodeData, IClinicalNoteDataAsync clinicalNoteData, ICRUD crud, IMiscData miscData,
            IAuditService audit, IApiController aPIController)
        {
            //_clinContext = context;
            //_cXContext = cXContext;
            //_apiContext = apiContext;
            _config = config;
            _staffUser = staffUserData;
            _hpo = new HPOVM();
            _hpoData = hPOCodeData;
            _clinicaNoteData = clinicalNoteData;
            _crud = crud;
            _misc = miscData;
            _audit = audit;
            _api = aPIController;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> HPOTerm(int id, string? searchTerm)
        {
            try
            {
                _hpo.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _hpo.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - HPO", "ID=" + id.ToString(), _ip.GetIPAddress());
                
                _hpo.clinicalNote = await _clinicaNoteData.GetClinicalNoteDetails(id);
                _hpo.hpoTermDetails = await _hpoData.GetExistingHPOTermsList(id);                
                _hpo.hpoExtractedTerms = await _hpoData.GetExtractedTermsList(id, _config);

                if(searchTerm != null) 
                {
                    List<APIControllers.Models.HPOTerm> termList = new List<APIControllers.Models.HPOTerm>();
                    termList = await _api.GetHPOCodes(searchTerm);
                    _hpo.hpoTerms = new List<ClinicalXPDataConnections.Models.HPOTerm>();
                    foreach (var item in termList)
                    {
                        _hpo.hpoTerms.Add(new ClinicalXPDataConnections.Models.HPOTerm { ID = item.ID, Term = item.Term, TermCode = item.TermCode });
                    }

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
        public async Task<IActionResult> AddHPOTerm(int noteID, string termCode) //add a HPO code from the list (available through the API)
        {
            try
            {
                _hpo.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _hpo.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - HPO", "NoteID=" + noteID.ToString(), _ip.GetIPAddress());
                //check if code exists, add it if it doesn't
                if (_hpoData.GetHPOTermByTermCode(termCode) == null)
                {                    
                    APIControllers.Models.HPOTerm term = await _api.GetHPODataByTermCode(termCode);
                    _hpoData.AddHPOTermToDatabase(termCode, term.Term, staffCode, _config);

                    //and add its synonyms as well
                    ClinicalXPDataConnections.Models.HPOTerm termAdded = await _hpoData.GetHPOTermByTermCode(termCode);
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
        public async Task<IActionResult> AddHPOTermFromText(int termID, int noteID) //add a HPO term found in the text
        {
            try
            {
                var term = await _hpoData.GetHPOTermByID(termID);
                string termCode = term.TermCode;


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
        public IActionResult DeleteHPOTermFromNote(int id)
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

        public async Task<IActionResult> GetAllHPOTerms() //refreshes the list from the API (NOT available to most users) 
        {
            string hpoTermGet = await _api.GetAllHPOTerms(User.Identity.Name);

            if (hpoTermGet == "success")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorHome", "Error", new { error = "Failed to get HPO terms", formName = "HPO-getall" });
            }
        }
    }
}
