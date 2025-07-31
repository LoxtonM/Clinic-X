using ClinicalXPDataConnections.Models;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class TestDiseaseVM
    {        
        public Patient patient { get; set; }
        public Test test { get; set; }
        public List<Test> tests { get; set; }
        public List<TestType> testList { get; set; }
        public Diagnosis diagnosis { get; set; }
        public List<Diagnosis> diagnosisList { get; set; }
        public List<Disease> diseaseList { get; set; }
        public List<DiseaseStatus> statusList { get; set; }
        public string? searchTerm { get; set; }
        public int? ageOfTest { get; set; }
        public List<SampleTypes> sampleTypes { get; set; }
        public List<SampleRequirements> sampleRequirementList { get; set; }
        public BloodForm bloodForm { get; set; }
        public List<BloodForm> bloodFormList { get; set; }
        public string edmsLink { get; set; }
        public string phenotipsLink { get; set; }
        public bool isPatientInPhenotips { get; set; }
    }
}
