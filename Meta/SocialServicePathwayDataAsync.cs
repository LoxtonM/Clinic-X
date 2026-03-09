using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface ISocialServicePathwayDataAsync
    {
        public Task<List<SocialServicePathway>> GetSocialServicePathwayList(int icpID);
    }

    public class SocialServicePathwayDataAsync : ISocialServicePathwayDataAsync
    {
        private readonly ClinicXContext _cxContext;

        public SocialServicePathwayDataAsync(ClinicXContext cXContext)
        {
            _cxContext = cXContext;
        }

        public async Task<List<SocialServicePathway>> GetSocialServicePathwayList(int icpID)
        {
            IQueryable<SocialServicePathway> ssp = _cxContext.SocialServicePathway.Where(s => s.ICPID == icpID);

            return await ssp.ToListAsync();
        }

    }
}
