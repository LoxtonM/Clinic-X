using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface ICancerRequestDataAsync
    {
        public Task<List<CancerRequests>> GetCancerRequestsList();
        public Task<CancerRequests> GetCancerRequestDetail(int id);
    }
    public class CancerRequestDataAsync : ICancerRequestDataAsync
    {
        private readonly ClinicXContext _clinContext;
        
        public CancerRequestDataAsync(ClinicXContext context)
        {
            _clinContext = context;           
        }
        
        public async Task<List<CancerRequests>> GetCancerRequestsList()
        {
            IQueryable<CancerRequests> requests = from r in _clinContext.CancerRequests
                         where r.InUse == true
                         orderby r.ID
                         select r;
           
            return await requests.ToListAsync();
        }

        public async Task<CancerRequests> GetCancerRequestDetail(int id)
        {
            CancerRequests request = await _clinContext.CancerRequests.FirstAsync(r => r.ID == id);

            return request;
        }
    }
}
