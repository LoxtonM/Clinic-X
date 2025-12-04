using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using System.Security.Cryptography.Xml;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class CaseloadVM
    {
        public List<Caseload> caseLoad { get; set; }
        public List<StaffMember> clinicians { get; set; }
        public int countClinics { get; set; }
        public int countTriages { get; set; }
        public int countCancerICPs { get; set; }
        public int countTests { get; set; }
        public int countReviews { get; set; }
        public int countLetters { get; set; }
        public string name { get; set; }
        public string staffCode { get; set; }
        public bool isSupervisor { get; set; }
        public bool isLive { get; set; }
        public string notificationMessage { get; set; }
        public string dllVersion { get; set; }
        public string appVersion { get; set; }
        public Dictionary<string, int> TotalConsReferralCount { get; set; }
        public Dictionary<string, int> TotalConsReferralCountGeneral { get; set; }
        public Dictionary<string, int> TotalConsReferralCountCancer { get; set; }
        public Dictionary<string, int> TotalGCReferralCount { get; set; }
        public Dictionary<string, int> TotalGCReferralCountGeneral { get; set; }
        public Dictionary<string, int> TotalGCReferralCountCancer { get; set; }
        public Dictionary<string, int> TotalAreaReferralCount { get; set; }
        public Dictionary<string, int> TotalAreaReferralCountGeneral { get; set; }
        public Dictionary<string, int> TotalAreaReferralCountCancer { get; set; }
        public int yearSelected { get; set; }
        public string pathwaySelected { get; set; }
        public string regionCodeSelected { get; set; }
        public string regionNameSelected { get; set; }
        public List<int> years { get; set; }
        public List<Pathway> pathways { get; set; }
        public Dictionary<string, string> geographicalRegions { get; set; }
    }
}
