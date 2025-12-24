using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IGeneCodeDataAsync
    {
        public Task<List<GeneCode>> GetGeneCodeList();
    }
    public class GeneCodeDataAsync : IGeneCodeDataAsync
    {
        private readonly ClinicXContext _clinContext;

        public GeneCodeDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }

        public async Task<List<GeneCode>> GetGeneCodeList()
        {
            IQueryable<GeneCode> geneCodes= from g in _clinContext.GeneCode
                                              where g.InUse == true
                          orderby g.TestCode
                         select g;

            return await geneCodes.ToListAsync();
        }

        
    }
}
