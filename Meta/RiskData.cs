using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class RiskData
    {
        private readonly ClinicalContext _clinContext;

        public RiskData(ClinicalContext context)
        {
            _clinContext = context;            
        }

        public List<Risk> GetRiskList(int? icpID) //Get list of all risk items for an ICP (by IcpID)
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == icpID);

            var risks = from r in _clinContext.Risk
                       where r.MPI == icp.MPI
                       select r;
           
            return risks.ToList();
        }       

        public Risk GetRiskDetails(int? riskID) //Get details of risk item by RiskID
        {
            var risk = _clinContext.Risk.FirstOrDefault(c => c.RiskID == riskID);
            return risk;
        }

        public List<RiskCodes> GetRiskCodesList()
        {
            var item = from i in _clinContext.RiskCodes
                       orderby i.RiskCode
                       select i;

            return item.ToList();
        }

        public List<CalculationTools> GetCalculationToolsList()
        {
            var item = from i in _clinContext.CalculationTools
                       select i;

            return item.ToList();
        }

    }
}
