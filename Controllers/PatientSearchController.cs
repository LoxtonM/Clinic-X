using Microsoft.AspNetCore.Mvc;
//using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using ClinicX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;

namespace ClinicX.Controllers
{
    public class PatientSearchController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly PatientSearchVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientSearchData _patientSearchData;
        private readonly IAuditService _audit;
        

        public PatientSearchController(IConfiguration config, IStaffUserData staffUserData, IPatientSearchData patientSearchData, IAuditService auditService)
        {
            //_clinContext = context;
            _config = config;
            _pvm = new PatientSearchVM();
            //_staffUser = new StaffUserData(_clinContext);
            _staffUser = staffUserData;
            //_patientSearchData = new PatientSearchData(_clinContext);            
            _patientSearchData = patientSearchData;
            //_audit = new AuditService(_config);
            _audit = auditService;
        }

        [Authorize]
        public IActionResult Index(string? cguNo, string? firstname, string? lastname, string? nhsNo, DateTime? dob)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                string searchTerm = "";
                _pvm.staffCode = staffCode;

                if (cguNo != null || firstname != null || lastname != null || nhsNo != null || (dob != null && dob != DateTime.Parse("0001-01-01")))
                {
                    _pvm.patientsList = new List<Patient>(); //because null

                    if (cguNo != null) 
                    {
                        if (cguNo != ".") //to stop searching everything by looking for "."
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByCGUNo(cguNo);
                        }
                        searchTerm = "CGU_No=" + cguNo;
                        _pvm.cguNumberSearch = cguNo;
                    }
                    if (nhsNo != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByNHS(nhsNo);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.SOCIAL_SECURITY == nhsNo).ToList();
                        }
                        searchTerm = searchTerm + "," + "NHSNo=" + nhsNo;
                        _pvm.nhsNoSearch = nhsNo;
                    }
                    
                    if (lastname != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByName(null, lastname);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.LASTNAME.ToUpper().Contains(lastname.ToUpper())).ToList();
                        }
                        searchTerm = searchTerm + "," + "Surname=" + lastname;
                        _pvm.surnameSearch = lastname;
                    }

                    if (firstname != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByName(firstname, null);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.FIRSTNAME.ToUpper().Contains(firstname.ToUpper())).ToList();
                        }
                        searchTerm = searchTerm + "," + "Forename=" + firstname;
                        _pvm.forenameSearch = firstname;
                    }

                    if (dob != null && dob != DateTime.Parse("0001-01-01")) //all the different ways of DOB being null...
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByDOB(dob.GetValueOrDefault());
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.DOB == dob).ToList();
                        }
                        searchTerm = searchTerm + "," + "DOB=" + dob.ToString();
                        _pvm.dobSearch = dob.GetValueOrDefault();
                    }

                    _pvm.patientsList = _pvm.patientsList.OrderBy(p => p.LASTNAME).ThenBy(p => p.FIRSTNAME).ToList();
                }

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Patient Search", searchTerm, _ip.GetIPAddress());

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "PatientSearch" });
            }
        }    
        
        public IActionResult ViewAllMyPatients(string staffCode)
        {
            _pvm.patientsList = _patientSearchData.GetPatientsListByStaffCode(staffCode).DistinctBy(p => p.MPI).ToList();

            return View(_pvm);
        }        
    }
}
