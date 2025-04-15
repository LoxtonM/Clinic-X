using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class RiskSurveillanceVM
    {
        public Patient patient { get; set; }
        public ICPCancer icpCancer { get; set; }
        public Risk riskDetails { get; set; }
        public List<Risk> riskList { get; set; }
        public Surveillance surveillanceDetails { get; set; }
        public List<Surveillance> surveillanceList { get; set; }
        public List<Eligibility> eligibilityList { get; set; }
        public Eligibility eligibilityDetails { get; set; }
        public List<RiskCodes> riskCodes { get; set; }
        public List<SurvSiteCodes> survSiteCodes { get; set; }
        public List<StaffMember> staffMembersList { get; set; }
        public List<CalculationTool> calculationTools {  get; set; }
        public List<SurvTypeCodes> survTypeCodes { get; set; }
        public List<SurvFreqCodes> survFreqCodes { get; set; }
        public List<DiscontinuedReasonCodes> discontinuedReasonCodes { get; set; }
        public List<GeneChange> geneChange { get; set; }
        public List<GeneCode> geneCode { get; set; }
        public List<Relative> relatives { get; set; }
        public int refID { get; set; }
        public int riskID { get; set; }
        public string staffCode { get; set; }
    }
}
