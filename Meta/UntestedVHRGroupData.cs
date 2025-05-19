using ClinicX.Data;
using ClinicX.Models;

namespace ClinicX.Meta
{
    interface IUntestedVHRGroupData
    {
        public UntestedVHRGroup GetUntestedVHRGroupData(int refid);
    }
    public class UntestedVHRGroupData : IUntestedVHRGroupData
    {
        private readonly ClinicXContext _clinContext;

        public UntestedVHRGroupData(ClinicXContext context)
        {
            _clinContext = context;
        }

        public UntestedVHRGroup GetUntestedVHRGroupData(int refid)
        {
            List<UntestedVHRGroup> uvgList = _clinContext.UntestedVHRGroup.Where(v => v.RefID != null).ToList();

            UntestedVHRGroup uvg = uvgList.FirstOrDefault(v => v.RefID == refid);

            return uvg;
        }
    }
    //http://localhost:7168/Triage/VHRPro?id=40558
}
