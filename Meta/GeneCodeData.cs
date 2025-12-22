using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public interface IGeneCodeData
    {
        public List<GeneCode> GetGeneCodeList();
    }
    public class GeneCodeData : IGeneCodeData
    {
        private readonly ClinicXContext _clinContext;

        public GeneCodeData(ClinicXContext context)
        {
            _clinContext = context;
        }

        public List<GeneCode> GetGeneCodeList()
        {
            IQueryable<GeneCode> geneCodes= from g in _clinContext.GeneCode
                                              where g.InUse == true
                          orderby g.TestCode
                         select g;

            return geneCodes.ToList();
        }

        
    }
}
