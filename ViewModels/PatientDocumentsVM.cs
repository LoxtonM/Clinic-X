using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    public class PatientDocumentsVM
    {
        public string folderPath { get; set; }
        public Patient patient { get; set; }
        public List<string> fileNames { get; set; }
    }
}
