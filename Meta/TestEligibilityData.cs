using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ITestEligibilityData
    {
        public List<Eligibility> GetTestingEligibilityList(int? mpi);
    }
    public class TestEligibilityData : ITestEligibilityData
    {
        private readonly ClinicXContext _clinContext;

        public TestEligibilityData(ClinicXContext context)
        {
            _clinContext = context;
        }
                

        public List<Eligibility> GetTestingEligibilityList(int? mpi) //Get list of testing aligibility codes by IcpID
        {
            IQueryable<Eligibility> eligibilities = from e in _clinContext.Eligibility
                              where e.MPI == mpi
                              select e;

            return eligibilities.ToList();
        }
        
    }
}
