using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IRelativeDiagnosisData
    {
        public List<RelativesDiagnosis> GetRelativeDiagnosisList(int id);
        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id);
        public List<CancerReg> GetCancerRegList();
        public List<RequestStatus> GetRequestStatusList();
        public List<TumourSite> GetTumourSiteList();
        public List<TumourLat> GetTumourLatList();
        public List<TumourMorph> GetTumourMorphList();
    }
    public class RelativeDiagnosisData : IRelativeDiagnosisData
    {
        private readonly ClinicalContext _clinContext;

        public RelativeDiagnosisData(ClinicalContext context)
        {
            _clinContext = context;
        }        

        public List<RelativesDiagnosis> GetRelativeDiagnosisList(int id)
        {
            IQueryable<RelativesDiagnosis> reldiag = from r in _clinContext.RelativesDiagnoses
                           where r.RelsID == id
                           select r;
            
            return reldiag.ToList();
        }

        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id)
        {
            RelativesDiagnosis item = _clinContext.RelativesDiagnoses.FirstOrDefault(rd => rd.TumourID == id);
            return item;
        }

        public List<CancerReg> GetCancerRegList()
        {
            IQueryable<CancerReg> creg = from c in _clinContext.CancerReg
                       where c.Creg_InUse == true
                       select c;
            int dfs = creg.Count();
            return creg.ToList();
        }

        public List<RequestStatus> GetRequestStatusList()
        {
            IQueryable<RequestStatus> status = from s in _clinContext.RequestStatus
                         select s;
            int dfs = status.Count();
            return status.ToList();
        }

        public List<TumourSite> GetTumourSiteList()
        {
            IQueryable<TumourSite> item = from i in _clinContext.TumourSite
                       select i;

            return item.ToList();
        }

        public List<TumourLat> GetTumourLatList()
        {
            IQueryable<TumourLat> item = from i in _clinContext.TumourLat
                       select i;

            return item.ToList();
        }

        public List<TumourMorph> GetTumourMorphList()
        {
            IQueryable<TumourMorph> item = from i in _clinContext.TumourMorph
                       select i;

            return item.ToList();
        }

        

    }
}
