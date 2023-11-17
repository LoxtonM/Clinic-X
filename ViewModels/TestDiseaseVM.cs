using ClinicX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ClinicX.Models.Relatives;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class TestDiseaseVM
    {        
        public Patients patient { get; set; }
        public Test Test { get; set; }
        public List<TestList> testList { get; set; }
        public Diagnosis Diagnosis { get; set; }
        public List<DiseaseList> diseaseList { get; set; }
        public List<DiseaseStatusList> statusList { get; set; }

    }
}
