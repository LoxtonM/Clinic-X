using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class CaseloadData 
    {
        private readonly ClinicalContext _clinContext;       

        public CaseloadData(ClinicalContext context)
        {
            _clinContext = context;
        }
        
        public List<Caseload> GetCaseloadList(string staffCode) //Get caseload for clinician
        {
            var caseload = from c in _clinContext.Caseload
                           where c.StaffCode == staffCode
                           select c;

            return caseload.ToList();
        }

    }
}
