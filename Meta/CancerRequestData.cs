using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ICancerRequestData
    {
        public List<CancerRequests> GetCancerRequestsList();
        public CancerRequests GetCancerRequestDetail(int id);
    }
    public class CancerRequestData : ICancerRequestData
    {
        private readonly ClinicalContext _clinContext;
        
        public CancerRequestData(ClinicalContext context)
        {
            _clinContext = context;           
        }
        
        public List<CancerRequests> GetCancerRequestsList()
        {
            IQueryable<CancerRequests> requests = from r in _clinContext.CancerRequests
                         where r.InUse == true
                         orderby r.ID
                         select r;
           
            return requests.ToList();
        }

        public CancerRequests GetCancerRequestDetail(int id)
        {
            CancerRequests request = _clinContext.CancerRequests.FirstOrDefault(r => r.ID == id);

            return request;
        }
    }
}
