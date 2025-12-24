using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IBreastHistoryDataAsync
    {
        public Task<BreastSurgeryHistory> GetBreastSurgeryHistory(int mpi);
    }
    public class BreastHistoryDataAsync : IBreastHistoryDataAsync
    {
        private readonly ClinicXContext _clinContext;

        public BreastHistoryDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }
                
        public async Task<BreastSurgeryHistory> GetBreastSurgeryHistory(int mpi)
        {
            BreastSurgeryHistory bish = await _clinContext.BreastSurgeryHistory.FirstAsync(d => d.MPI == mpi);

            return bish;
        }
    }
}
