using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public interface ISampleDataAsync
    {
        public Task<List<SampleTypes>> GetSampleTypeList();
        public Task<List<SampleRequirements>> GetSampleRequirementsList();
        
    }
    public class SampleDataAsync : ISampleDataAsync
    {
        private readonly ClinicXContext _cxContext;

        public SampleDataAsync(ClinicXContext cXContext)
        {            
            _cxContext = cXContext;
        }

        public async Task<List<SampleTypes>> GetSampleTypeList()
        {
            IQueryable<SampleTypes> items = from i in _cxContext.SampleTypes
                                                   select i;

            return items.ToList();
        }

        public async Task<List<SampleRequirements>> GetSampleRequirementsList()
        {
            IQueryable<SampleRequirements> items = from i in _cxContext.SampleRequirements
                        select i;           

            return items.ToList();
        }
               
        

    }
}
