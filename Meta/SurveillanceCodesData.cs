using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public interface ISurveillanceCodesData
    {
        public List<SurvSiteCodes> GetSurvSiteCodesList();
        public List<SurvTypeCodes> GetSurvTypeCodesList();
        public List<SurvFreqCodes> GetSurvFreqCodesList();
        public List<DiscontinuedReasonCodes> GetDiscReasonCodesList();
    }
    public class SurveillanceCodesData : ISurveillanceCodesData
    {
        private readonly ClinicXContext _clinContext;
        
        public SurveillanceCodesData(ClinicXContext context)
        {
            _clinContext = context;        
        }

        public List<SurvSiteCodes> GetSurvSiteCodesList()
        {
            IQueryable<SurvSiteCodes> item = from i in _clinContext.SurvSiteCodes
                                             orderby i.SurvSite
                                             select i;

            return item.ToList();
        }

        public List<SurvTypeCodes> GetSurvTypeCodesList()
        {
            IQueryable<SurvTypeCodes> item = from i in _clinContext.SurvTypeCodes
                                             orderby i.SurvType
                                             select i;

            return item.ToList();
        }

        public List<SurvFreqCodes> GetSurvFreqCodesList()
        {
            IQueryable<SurvFreqCodes> item = from i in _clinContext.SurvFreqCodes
                                             orderby i.SurvFreq
                                             select i;

            return item.ToList();
        }

        public List<DiscontinuedReasonCodes> GetDiscReasonCodesList()
        {
            IQueryable<DiscontinuedReasonCodes> item = from i in _clinContext.DiscontinuedReasonCodes
                                                       orderby i.SurvDiscReason
                                                       select i;

            return item.ToList();
        }

    }
}
