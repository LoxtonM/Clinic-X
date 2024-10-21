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
        private readonly ClinicalContext _clinContext;

        public BreastHistoryData(ClinicalContext context)
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
