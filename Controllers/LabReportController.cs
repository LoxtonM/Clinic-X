using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using ClinicX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class LabReportController : Controller
    {
        private readonly LabContext _context;
        private readonly ILabData _labData;
        private readonly LabReportVM _lvm;

        public LabReportController(LabContext context)
        {
            _context = context;
            _labData = new LabReportData(_context);
            _lvm = new LabReportVM();
        }

        

        public IActionResult LabPatientSearch(string? firstname, string? lastname, string? nhsno, string? postcode, DateTime? dob)
        {
            _lvm.patientsList = new List<LabPatient>();

            if (firstname != null || lastname != null || nhsno != null || postcode != null || dob != null)
            {
                _lvm.patientsList = _labData.GetPatients(firstname, lastname, nhsno, postcode, dob);
            }

            return View(_lvm);
        }

        public IActionResult LabReports(int intID)
        {
            _lvm.patient = _labData.GetPatientDetails(intID);
            _lvm.cytoReportsList = _labData.GetCytoReportsList(intID);
            //_lvm.dnaReportsList = _labData.GetDNAReportsList(intID);
            _lvm.labDNALabDataList = _labData.GetDNALabDataList(intID);

            return View(_lvm);
        }

        public IActionResult SampleDetails(string labno)
        {
            _lvm.cytoReport = _labData.GetCytoReport(labno);
            _lvm.dnaReport = _labData.GetDNAReport(labno);            

            return View(_lvm);
        }

        public IActionResult DNALabReport(string labno, string indication, string reason)
        {            
            _lvm.dnaReportDetails = _labData.GetDNAReportDetails(labno, indication, reason);
            _lvm.dnaReport = _labData.GetDNAReport(labno);            
    
            return View(_lvm);
        }

        public IActionResult CytoLabReport(string labno)
        {
            _lvm.cytoReport = _labData.GetCytoReport(labno);            

            return View(_lvm);
        }
    }
}
