using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IPriorityData
    {
        public List<Priority> GetPriorityList();
    }
    public class PriorityData : IPriorityData
    {
        private readonly ClinicalContext _clinContext;

        public PriorityData(ClinicalContext context)
        {
            _clinContext = context;
        }       

        public List<Priority> GetPriorityList()
        {
            var priority = from p in _clinContext.Priority
                          where p.IsActive == true
                          orderby p.PriorityLevel
                         select p;

            return priority.ToList();
        }

        
    }
}
