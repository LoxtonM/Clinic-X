using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ISampleData
    {
        public List<SampleTypes> GetSampleTypeList();
        public List<SampleRequirements> GetSampleRequirementsList();
        
    }
    public class SampleData : ISampleData
    {
        private readonly ClinicXContext _cxContext;

        public SampleData(ClinicXContext cXContext)
        {            
            _cxContext = cXContext;
        }

        public List<SampleTypes> GetSampleTypeList()
        {
            IQueryable<SampleTypes> items = from i in _cxContext.SampleTypes
                                                   select i;

            return items.ToList();
        }

        public List<SampleRequirements> GetSampleRequirementsList()
        {
            IQueryable<SampleRequirements> items = from i in _cxContext.SampleRequirements
                        select i;           

            return items.ToList();
        }
               
        

    }
}
