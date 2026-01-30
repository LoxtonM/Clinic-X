using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class PatientSearchVM
    { 
        public List<Patient> patientsList { get; set; }
        public string cguNumberSearch { get; set; }
        public string forenameSearch { get; set; }
        public string surnameSearch { get; set;}
        public DateTime dobSearch { get; set; }
        public string nhsNoSearch { get; set; }
        public string staffCode {  get; set; }
        public string telNoSearch { get; set; }
        public string emailSearch { get; set; }
    }
}
