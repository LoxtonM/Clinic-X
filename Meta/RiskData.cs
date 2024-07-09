using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IRiskData
    {
        public List<Risk> GetRiskList(int? icpID);
        public Risk GetRiskDetails(int? riskID);
        public List<Risk> GetRiskListByRefID(int? refID);
        public List<RiskCodes> GetRiskCodesList();
        public List<CalculationTool> GetCalculationToolsList();
    }
    public class RiskData : IRiskData
    {
        private readonly ClinicalContext _clinContext;

        public RiskData(ClinicalContext context)
        {
            _clinContext = context;            
        }

        public List<Risk> GetRiskList(int? icpID) //Get list of all risk items for an ICP (by IcpID)
        {
            ICPCancer icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == icpID);

            IQueryable<Risk> risks = from r in _clinContext.Risk
                       where r.MPI == icp.MPI
                       select r;
           
            return risks.ToList();
        }       

        public Risk GetRiskDetails(int? riskID) //Get details of risk item by RiskID
        {
            Risk risk = _clinContext.Risk.FirstOrDefault(c => c.RiskID == riskID);
            return risk;
        }

        public List<Risk> GetRiskListByRefID(int? refID) //Get details of risk item by RiskID
        {
            IQueryable<Risk> risk = _clinContext.Risk.Where(c => c.RefID == refID);
            return risk.ToList();
        }

        public List<RiskCodes> GetRiskCodesList()
        {
            IQueryable<RiskCodes> item = from i in _clinContext.RiskCodes
                       orderby i.RiskCode
                       select i;

            return item.ToList();
        }

        public List<CalculationTool> GetCalculationToolsList()
        {
            IQueryable<CalculationTool> item = from i in _clinContext.CalculationTools
                       select i;

            return item.ToList();
        }

    }
}
