using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ICPVM
    {
        public List<StaffMemberList> staffMembers { get; set; }
        public List<StaffMemberList> consultants { get; set; }
        public List<StaffMemberList> GCs { get; set; }
        public List<Triages> triages { get; set; }
        public List<ICPCancer> icpCancerList { get; set; }
        public Triages triage { get; set; }
        public ICPGeneral? icpGeneral { get; set; }
        public ICPCancer? icpCancer { get; set; }
        public List<ClinicalFacilityList> clinicalFacilityList { get; set; }
        public List<ICPActionsList> cancerActionsList { get; set; }
        public List<ICPGeneralActionsList> generalActionsList { get; set; }
        public List<ICPGeneralActionsList2> generalActionsList2 { get; set; }
        public List<ICPCancerReviewActionsList> cancerReviewActionsLists { get; set; }
        public ICPCancerReviewActionsList cancerAction {  get; set; }
        public List<Risk> riskList { get; set; }
        public Risk riskDetails { get; set; }
        public List<Surveillance> surveillanceList { get; set; }
        public Surveillance survDetails { get; set; }
        public Eligibility eligibility { get; set; }
        public List<Eligibility> eligibilityList { get; set; }
        public List<Documents> documentList { get; set; }
    }
}
