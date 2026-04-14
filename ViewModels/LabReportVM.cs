using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class LabReportVM
    {
        public LabPatient patient {  get; set; }
        public List<LabPatient> patientsList { get; set; }
        public LabDNALab dnaReport { get; set; }
        public List<LabDNALab> dnaReportsList { get; set; }
        public List<LabDNALabData> labDNALabDataList { get; set; }
        public LabLab cytoReport { get; set; }
        public List<LabLab> cytoReportsList { get; set; }
        public LabDNAReport dnaReportDetails { get; set; }
        public string? searchTerms { get; set; }
        public string? firstnameSearch { get; set; }
        public string? lastnameSearch { get; set; }
        public DateTime? dobSearch { get; set; }
        public string? nhsnoSearch { get; set; }
        public string? postcodeSearch { get; set; }
        public string? labNoSearch { get; set; }
    }
}
