using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IUntestedVHRGroupDataAsync
    {
        public Task<UntestedVHRGroup> GetUntestedVHRGroupData(int refid);
        public Task<UntestedVHRGroup> GetUntestedVHRGroupDataByRefID(int refid);
    }
    public class UntestedVHRGroupDataAsync : IUntestedVHRGroupDataAsync
    {
        private readonly ClinicXContext _clinContext;

        public UntestedVHRGroupDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }

        public async Task<UntestedVHRGroup> GetUntestedVHRGroupData(int id)
        {            
            UntestedVHRGroup uvg = await _clinContext.UntestedVHRGroup.FirstAsync(v => v.RelativeRiskID == id);

            return uvg;
        }

        public async Task<UntestedVHRGroup> GetUntestedVHRGroupDataByRefID(int refid)
        {
            IQueryable<UntestedVHRGroup> uvgList = _clinContext.UntestedVHRGroup.Where(v => v.RefID != null);

            UntestedVHRGroup uvg = await uvgList.FirstAsync(v => v.RefID == refid);

            return uvg;
        }
    }
}
