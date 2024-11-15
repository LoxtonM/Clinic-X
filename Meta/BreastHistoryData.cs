using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IBreastHistoryData
    {
        public BreastSurgeryHistory GetBreastSurgeryHistory(int mpi);
    }
    public class BreastHistoryData : IBreastHistoryData
    {
        private readonly ClinicXContext _clinContext;

        public BreastHistoryData(ClinicXContext context)
        {
            _clinContext = context;
        }
                
        public BreastSurgeryHistory GetBreastSurgeryHistory(int mpi)
        {
            BreastSurgeryHistory bish = _clinContext.BreastSurgeryHistory.FirstOrDefault(d => d.MPI == mpi);

            return bish;
        }
    }
}
