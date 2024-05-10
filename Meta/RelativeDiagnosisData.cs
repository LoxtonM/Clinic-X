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
            var reldiag = from r in _clinContext.RelativesDiagnoses
                           where r.RelsID == id
                           select r;
            
            return reldiag.ToList();
        }

        public RelativesDiagnosis GetRelativeDiagnosisDetails(int id)
        {
            var item = _clinContext.RelativesDiagnoses.FirstOrDefault(rd => rd.TumourID == id);
            return item;
        }

        public List<CancerReg> GetCancerRegList()
        {
            var creg = from c in _clinContext.CancerReg
                       where c.Creg_InUse == true
                       select c;
            int dfs = creg.Count();
            return creg.ToList();
        }

        public List<RequestStatus> GetRequestStatusList()
        {
            var status = from s in _clinContext.RequestStatus
                         select s;
            int dfs = status.Count();
            return status.ToList();
        }

        public List<TumourSite> GetTumourSiteList()
        {
            var item = from i in _clinContext.TumourSite
                       select i;

            return item.ToList();
        }

        public List<TumourLat> GetTumourLatList()
        {
            var item = from i in _clinContext.TumourLat
                       select i;

            return item.ToList();
        }

        public List<TumourMorph> GetTumourMorphList()
        {
            var item = from i in _clinContext.TumourMorph
                       select i;

            return item.ToList();
        }

        

    }
}
