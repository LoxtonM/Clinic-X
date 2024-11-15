using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ISurveillanceData
    {
        public List<Surveillance> GetSurveillanceList(int? mpi);
        public List<Surveillance> GetSurveillanceListByRiskID(int? riskID);
        public Surveillance GetSurvDetails(int? riskID);
        public List<SurvSiteCodes> GetSurvSiteCodesList();
        public List<SurvTypeCodes> GetSurvTypeCodesList();
        public List<SurvFreqCodes> GetSurvFreqCodesList();
        public List<DiscontinuedReasonCodes> GetDiscReasonCodesList();
    }
    public class SurveillanceData : ISurveillanceData
    {
        private readonly ClinicXContext _clinContext;
        
        public SurveillanceData(ClinicXContext context)
        {
            _clinContext = context;        
        }
        
        public List<Surveillance> GetSurveillanceList(int? mpi) //Get list of all surveillance recommendations for an ICP (by MPI)
        {
            IQueryable<Surveillance> surveillances = from r in _clinContext.Surveillance
                               where r.MPI == mpi
                               select r;

            return surveillances.ToList();
        }

        public List<Surveillance> GetSurveillanceListByRiskID(int? riskID) //Get list of all surveillance recommendations for a  risk item (by RiskID)
        {
            IQueryable<Surveillance> surveillances = from r in _clinContext.Surveillance
                                where r.RiskID == riskID
                                select r;

            return surveillances.ToList();
        }

        

        public Surveillance GetSurvDetails(int? survID) //Get details of surveillance recommendation by RiskID
        {
            Surveillance surv = _clinContext.Surveillance.FirstOrDefault(c => c.SurvRecID == survID);
            return surv;
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
