using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class RelativeDiagnosisVM
    {
        public Relatives relativeDetails { get; set; }
        public RelativesDiagnosis relativesDiagnosis { get; set; }
        public List<RelativesDiagnosis> relativesDiagnosisList { get; set; }
        public List<StaffMemberList> staffList { get; set; }
        public List<StaffMemberList> clinicianList { get; set; }
        public List<CancerReg> cancerRegList { get; set; }
        public List<RequestStatus> requestStatusList { get; set; }

    }
}
