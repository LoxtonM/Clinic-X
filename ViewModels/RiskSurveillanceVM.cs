using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class RiskSurveillanceVM
    {
        public Patients patient { get; set; }
        public ICPCancer icpCancer { get; set; }
        public Risk riskDetails { get; set; }
        public Surveillance surveillanceDetails { get; set; }
        public List<Surveillance> surveillanceList { get; set; }
        public List<Eligibility> eligibilityList { get; set; }
        public Eligibility eligibilityDetails { get; set; }
        public List<RiskCodes> riskCodes { get; set; }
        public List<SurvSiteCodes> survSiteCodes { get; set; }
        public List<StaffMemberList> staffMembersList { get; set; }
        public List<CalculationTools> calculationTools {  get; set; }
        public List<SurvTypeCodes> survTypeCodes { get; set; }
        public List<SurvFreqCodes> survFreqCodes { get; set; }
        public List<DiscontinuedReasonCodes> discontinuedReasonCodes { get; set; }
        public int refID { get; set; }
        public int riskID { get; set; }
    }
}
