using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IGeneChangeDataAsync
    {
        public Task<List<GeneChange>> GetGeneChangeList();
    }
    public class GeneChangeDataAsync : IGeneChangeDataAsync
    {
        private readonly ClinicXContext _clinContext;

        public GeneChangeDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }       

        public async Task<List<GeneChange>> GetGeneChangeList()
        {
            IQueryable<GeneChange> geneChanges = from g in _clinContext.GeneChange
                                              where g.Inuse == true
                          orderby g.GeneChangeDescription
                         select g;

            return await geneChanges.ToListAsync();
        }
    }
}
