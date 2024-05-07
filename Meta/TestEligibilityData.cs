using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class TestEligibilityData
    {
        private readonly ClinicalContext _clinContext;

        public TestEligibilityData(ClinicalContext context)
        {
            _clinContext = context;
        }
                

        public List<Eligibility> GetTestingEligibilityList(int? mpi) //Get list of testing aligibility codes by IcpID
        {
            var eligibilities = from e in _clinContext.Eligibility
                              where e.MPI == mpi
                              select e;

            return eligibilities.ToList();
        }
        
    }
}
