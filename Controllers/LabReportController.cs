//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class LabReportController : Controller
    {
        //private readonly LabContext _context;
        //private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly ILabDataAsync _labData;
        private readonly LabReportVM _lvm;
        private readonly IStaffUserDataAsync _staff;
        private readonly IAuditService _audit;

        public LabReportController(IConfiguration config, IStaffUserDataAsync staffUserData, IAuditService auditService, ILabDataAsync labData)
        {
            //_context = context;
            //_clinContext = clinContext;
            _config = config;
            _labData = labData;
            _lvm = new LabReportVM();
            _staff = staffUserData;
            _audit = auditService;
        }


        [Authorize]
        public async Task<IActionResult> LabPatientSearch(string? firstname, string? lastname, string? nhsno, string? postcode, DateTime? dob)
        {
            var user = await _staff.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = user.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "ClinicX - LabReports", "", _ip.GetIPAddress());

            _lvm.patientsList = new List<LabPatient>();

            if (firstname != null || lastname != null || nhsno != null || postcode != null || dob != null)
            {
                _lvm.patientsList = await _labData.GetPatients(firstname, lastname, nhsno, postcode, dob);
                _lvm.searchTerms = "Firstname:" + firstname + ",Lastname:" + lastname + ",NHSNo:" + nhsno + ",Postcode:" + postcode;
                if(dob != null)
                {
                    _lvm.searchTerms = _lvm.searchTerms + dob.Value.ToString("yyyy-MM-dd");
                }
            }

            return View(_lvm);
        }

        [Authorize]
        public async Task<IActionResult> LabReports(int intID)
        {
            _lvm.patient = await _labData.GetPatientDetails(intID);
            _lvm.cytoReportsList = await _labData.GetCytoReportsList(intID);
            //_lvm.dnaReportsList = _labData.GetDNAReportsList(intID);
            _lvm.labDNALabDataList = await _labData.GetDNALabDataList(intID);

            return View(_lvm);
        }

        [Authorize]
        public async Task<IActionResult> SampleDetails(string labno)
        {
            _lvm.cytoReport = await _labData.GetCytoReport(labno);
            _lvm.dnaReport = await _labData.GetDNAReport(labno);            

            return View(_lvm);
        }

        [Authorize]
        public async Task<IActionResult> DNALabReport(string labno, string indication, string reason) //needs more than the LabNo to get a specific report
        {            
            _lvm.dnaReportDetails = await _labData.GetDNAReportDetails(labno, indication, reason);
            _lvm.dnaReport = await _labData.GetDNAReport(labno);            
    
            return View(_lvm);
        }

        [Authorize]
        public async Task<IActionResult> CytoLabReport(string labno)
        {
            _lvm.cytoReport = await _labData.GetCytoReport(labno);

            return View(_lvm);
        }
    }
}
