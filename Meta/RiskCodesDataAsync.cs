using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IRiskCodesDataAsync
    {
        public Task<List<RiskCodes>> GetRiskCodesList();
        public Task<List<CalculationTool>> GetCalculationToolsList();
    }
    public class RiskCodesDataAsync : IRiskCodesDataAsync
    {
        private readonly ClinicXContext _clinContext;
        
        public RiskCodesDataAsync(ClinicXContext context)
        {
            _clinContext = context;            
        }

        public async Task<List<RiskCodes>> GetRiskCodesList()
        {
            IQueryable<RiskCodes> item = from i in _clinContext.RiskCodes
                                         orderby i.RiskCode
                                         select i;

            return await item.ToListAsync();
        }

        public async Task<List<CalculationTool>> GetCalculationToolsList()
        {
            IQueryable<CalculationTool> item = from i in _clinContext.CalculationTools
                                               select i;

            return await item.ToListAsync();
        }

    }
}
