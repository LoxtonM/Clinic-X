using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public interface IRiskCodesData
    {
        public List<RiskCodes> GetRiskCodesList();
        public List<CalculationTool> GetCalculationToolsList();
    }
    public class RiskCodesData : IRiskCodesData
    {
        private readonly ClinicXContext _clinContext;
        
        public RiskCodesData(ClinicXContext context)
        {
            _clinContext = context;            
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
