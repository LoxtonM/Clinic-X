using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface ISurveillanceCodesDataAsync
    {
        public Task<List<SurvSiteCodes>> GetSurvSiteCodesList();
        public Task<List<SurvTypeCodes>> GetSurvTypeCodesList();
        public Task<List<SurvFreqCodes>> GetSurvFreqCodesList();
        public Task<List<DiscontinuedReasonCodes>> GetDiscReasonCodesList();
    }
    public class SurveillanceCodesDataAsync : ISurveillanceCodesDataAsync
    {
        private readonly ClinicXContext _clinContext;
        
        public SurveillanceCodesDataAsync(ClinicXContext context)
        {
            _clinContext = context;        
        }

        public async Task<List<SurvSiteCodes>> GetSurvSiteCodesList()
        {
            IQueryable<SurvSiteCodes> item = from i in _clinContext.SurvSiteCodes
                                             orderby i.SurvSite
                                             select i;

            return await item.ToListAsync();
        }

        public async Task<List<SurvTypeCodes>> GetSurvTypeCodesList()
        {
            IQueryable<SurvTypeCodes> item = from i in _clinContext.SurvTypeCodes
                                             orderby i.SurvType
                                             select i;

            return await item.ToListAsync();
        }

        public async Task<List<SurvFreqCodes>> GetSurvFreqCodesList()
        {
            IQueryable<SurvFreqCodes> item = from i in _clinContext.SurvFreqCodes
                                             orderby i.SurvFreq
                                             select i;

            return await item.ToListAsync();
        }

        public async Task<List<DiscontinuedReasonCodes>> GetDiscReasonCodesList()
        {
            IQueryable<DiscontinuedReasonCodes> item = from i in _clinContext.DiscontinuedReasonCodes
                                                       orderby i.SurvDiscReason
                                                       select i;

            return await item.ToListAsync();
        }

    }
}
