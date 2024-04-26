using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class TestDiseaseVM
    {        
        public Patients patient { get; set; }
        public Test test { get; set; }
        public List<Test> tests { get; set; }
        public List<TestList> testList { get; set; }
        public Diagnosis diagnosis { get; set; }
        public List<Diagnosis> diagnosisList { get; set; }
        public List<DiseaseList> diseaseList { get; set; }
        public List<DiseaseStatusList> statusList { get; set; }

    }
}
