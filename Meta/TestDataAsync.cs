using ClinicalXPDataConnections.Data;
using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface ITestDataAsync
    {
        public Task<List<TestType>> GetTestList();
        public Task<List<Test>> GetTestListByUser(string username);
        public Task<List<Test>> GetTestListByPatient(int mpi);
        public Task<Test> GetTestDetails(int id);
    }
    public class TestDataAsync : ITestDataAsync
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cxContext;

        public TestDataAsync(ClinicalContext context, ClinicXContext cXContext)
        {
            _clinContext = context;
            _cxContext = cXContext;
        }
        

        public async Task<List<TestType>> GetTestList() //Get list of all available tests
        {
            IQueryable<TestType> items = from i in _cxContext.Tests
                        select i;           

            return await items.ToListAsync();
        }
               
        public async Task<List<Test>> GetTestListByUser(string username)
        {
            var staffMember = await _clinContext.StaffMembers.FirstAsync(s => s.EMPLOYEE_NUMBER == username);
            string staffCode = staffMember.STAFF_CODE;
             
            IQueryable<Test> tests = from t in _cxContext.Test
                        where t.ORDEREDBY.Equals(staffCode) & t.COMPLETE == "No"
                        select t;

            return await tests.ToListAsync();
        }

        public async Task<List<Test>> GetTestListByPatient(int mpi)
        {
            IQueryable<Test> tests = from t in _cxContext.Test
                        where t.MPI.Equals(mpi)
                        select t;

            return await tests.ToListAsync();
        }

        public async Task<Test> GetTestDetails(int id)
        {
            Test test = await _cxContext.Test.FirstAsync(t => t.TestID == id);

            return test;
        }

    }
}
