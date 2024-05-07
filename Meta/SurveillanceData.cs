using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class SurveillanceData
    {
        private readonly ClinicalContext _clinContext;
        
        public SurveillanceData(ClinicalContext context)
        {
            _clinContext = context;        
        }
        

        
        

       

        public List<Surveillance> GetSurveillanceList(int? mpi) //Get list of all surveillance recommendations for an ICP (by MPI)
        {
            var surveillances = from r in _clinContext.Surveillance
                               where r.MPI == mpi
                               select r;

            return surveillances.ToList();
        }

        public List<Surveillance> GetSurveillanceListByRiskID(int? riskID) //Get list of all surveillance recommendations for a  risk item (by RiskID)
        {
            var surveillances = from r in _clinContext.Surveillance
                                where r.RiskID == riskID
                                select r;

            return surveillances.ToList();
        }

        

        public Surveillance GetSurvDetails(int? riskID) //Get details of surveillance recommendation by RiskID
        {
            var surv = _clinContext.Surveillance.FirstOrDefault(c => c.RiskID == riskID);
            return surv;
        }


        public List<SurvSiteCodes> GetSurvSiteCodesList()
        {
            var item = from i in _clinContext.SurvSiteCodes
                       orderby i.SurvSite
                       select i;

            return item.ToList();
        }

        public List<SurvTypeCodes> GetSurvTypeCodesList()
        {
            var item = from i in _clinContext.SurvTypeCodes
                       orderby i.SurvType
                       select i;

            return item.ToList();
        }

        public List<SurvFreqCodes> GetSurvFreqCodesList()
        {
            var item = from i in _clinContext.SurvFreqCodes
                       orderby i.SurvFreq
                       select i;

            return item.ToList();
        }

        public List<DiscontinuedReasonCodes> GetDiscReasonCodesList()
        {
            var item = from i in _clinContext.DiscontinuedReasonCodes
                       orderby i.SurvDiscReason
                       select i;

            return item.ToList();
        }

    }
}
