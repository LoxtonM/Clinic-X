using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ICPVM
    {
        public List<StaffMember> staffMembers { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> GCs { get; set; }
        public List<Triages> triages { get; set; }
        public List<ICPCancer> icpCancerListOwn { get; set; }
        public List<ICPCancer> icpCancerListOther { get; set; }
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
        public Referrals referralDetails { get; set; }
        public List<Pathway> pathways { get; set; }
        public List<Priority> priorityList { get; set; }
        public string staffCode { get; set; }
        public string loggedOnUserType { get; set; }
        public bool isICPTriageStarted { get; set; }
    }
}
