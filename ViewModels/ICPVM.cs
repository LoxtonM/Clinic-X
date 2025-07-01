using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ICPVM
    {
        public Patient patient {  get; set; }
        public List<StaffMember> staffMembers { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> GCs { get; set; }
        public List<Triage> triages { get; set; }
        public List<ICPCancer> icpCancerListOwn { get; set; }
        public List<ICPCancer> icpCancerListOther { get; set; }
        public Triage triage { get; set; }
        public ICPGeneral? icpGeneral { get; set; }
        public ICPCancer? icpCancer { get; set; }
        public List<ClinicVenue> clinicalFacilityList { get; set; }
        public List<ICPAction> cancerActionsList { get; set; }
        public List<ICPGeneralAction> generalActionsList { get; set; }
        public List<ICPGeneralAction2> generalActionsList2 { get; set; }
        public List<ICPCancerReviewAction> cancerReviewActionsLists { get; set; }
        public ICPCancerReviewAction cancerAction {  get; set; }
        public List<Risk> riskList { get; set; }
        public Risk riskDetails { get; set; }
        public List<Surveillance> surveillanceList { get; set; }
        public Surveillance survDetails { get; set; }
        public Eligibility eligibility { get; set; }
        public List<Eligibility> eligibilityList { get; set; }
        public List<Document> documentList { get; set; }
        public Referral referralDetails { get; set; }
        public List<Pathway> pathways { get; set; }
        public List<Priority> priorityList { get; set; }
        public List<Relative> relatives { get; set; }
        public List<CancerRequests> cancerRequestsList { get; set; }
        public CancerRequests cancerRequest { get; set; }
        public List<ExternalCliniciansAndFacilities> clinicians { get; set; }
        //public List<ScreeningService> screeningCoordinators { get; set; }
        public List<RelativesDiagnosis> relativesDiagnoses { get; set; }
        public List<Leaflet> leaflets { get; set; }
        public StaffOptions staffOptions { get; set; }
        public List<string> specialities { get; set; }
        public string staffCode { get; set; }
        public string loggedOnUserType { get; set; }
        public bool isICPTriageStarted { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public int? referralAgeDays { get; set; }
        public int? referralAgeWeeks { get; set; }
        //public string? defaultScreeningCo {  get; set; }
        public bool isChild { get; set; }
        public string edmsLink { get; set; }
        public string canriskLink { get; set; }
        public string genomicsTestDirectoryLink { get; set; }
        public DateTime dobAt16 { get; set; }
    }
}
