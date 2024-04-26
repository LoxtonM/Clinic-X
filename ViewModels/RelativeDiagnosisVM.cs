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
        public List<TumourSite> tumourSiteList { get;set; }
        public List<TumourLat> tumourLatList { get; set; }
        public List<TumourMorph> tumourMorphList { get; set; }
        public List<Relation> relationList { get; set; }
        public List<Gender> genderList { get; set; }
        public int MPI { get; set; }
        public int WMFACSID { get; set; }

    }
}
