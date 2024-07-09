using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ITestData
    {
        public List<TestType> GetTestList();
        public List<Test> GetTestListByUser(string username);
        public List<Test> GetTestListByPatient(int mpi);
        public Test GetTestDetails(int id);
    }
    public class TestData : ITestData
    {
        private readonly ClinicalContext _clinContext;
    

        public TestData(ClinicalContext context)
        {
            _clinContext = context;
        }
        

        public List<TestType> GetTestList() //Get list of all available tests
        {
            IQueryable<TestType> items = from i in _clinContext.Tests
                        select i;           

            return items.ToList();
        }
               
        public List<Test> GetTestListByUser(string username)
        {
            string staffCode = _clinContext.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == username).STAFF_CODE;
             
            IQueryable<Test> tests = from t in _clinContext.Test
                        where t.ORDEREDBY.Equals(staffCode) & t.COMPLETE == "No"
                        select t;

            return tests.ToList();
        }

        public List<Test> GetTestListByPatient(int mpi)
        {
            IQueryable<Test> tests = from t in _clinContext.Test
                        where t.MPI.Equals(mpi)
                        select t;

            return tests.ToList();
        }

        public Test GetTestDetails(int id)
        {
            Test test = _clinContext.Test.FirstOrDefault(t => t.TestID == id);

            return test;
        }

    }
}
