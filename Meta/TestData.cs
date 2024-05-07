using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class TestData
    {
        private readonly ClinicalContext _clinContext;
    

        public TestData(ClinicalContext context)
        {
            _clinContext = context;
        }
        

        public List<TestList> GetTestList() //Get list of all available tests
        {
            var items = from i in _clinContext.Tests
                        select i;           

            return items.ToList();
        }
               
        public List<Test> GetTestListByUser(string username)
        {
            string staffCode = _clinContext.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == username).STAFF_CODE;
             
            var tests = from t in _clinContext.Test
                        where t.ORDEREDBY.Equals(staffCode) & t.COMPLETE == "No"
                        select t;

            return tests.ToList();
        }

        public List<Test> GetTestListByPatient(int mpi)
        {
            var tests = from t in _clinContext.Test
                        where t.MPI.Equals(mpi)
                        select t;

            return tests.ToList();
        }

        public Test GetTestDetails(int id)
        {
            var test = _clinContext.Test.FirstOrDefault(t => t.TestID == id);

            return test;
        }

    }
}
