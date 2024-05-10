using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface ITriageData
    {
        public ICP GetICPDetails(int icpID);
        public Triages GetTriageDetails(int? icpID);
        public List<Triages> GetTriageList(string username);
        public List<ICPCancer> GetCancerICPList(string username);        
        public List<ClinicalFacilityList> GetClinicalFacilitiesList();
        public ICPGeneral GetGeneralICPDetails(int? icpID);
        public ICPCancer GetCancerICPDetails(int? icpID);
        public ICPCancer GetCancerICPDetailsByICPID(int? icpID);
        public int GetGeneralICPCountByICPID(int id);
        public int GetCancerICPCountByICPID(int id);
    }
    public class TriageData : ITriageData
    {
        private readonly ClinicalContext _clinContext;
        
        public TriageData(ClinicalContext context)
        {
            _clinContext = context;           
        }
        
                
        public ICP GetICPDetails(int icpID)
        {
            var icp = _clinContext.ICP.FirstOrDefault(i => i.ICPID == icpID);
            return icp;
        }
        public Triages GetTriageDetails(int? icpID) //Get details of ICP from the IcpID
        {
            var icp = _clinContext.Triages.FirstOrDefault(i => i.ICPID == icpID);
            return icp;
        }

        public List<Triages> GetTriageList(string username) //Get list of all outstanding triages for a specific user (by login name)
        {
            var triages = from t in _clinContext.Triages
                         where t.LoginDetails == username
                         orderby t.RefDate descending
                         select t;           

            return triages.ToList();
        }

        public List<ICPCancer> GetCancerICPList(string username) //Get list of all open Cancer ICP Reviews for a specific user (by login name)
        {
            var user = _clinContext.StaffMembers.FirstOrDefault(s => s.EMPLOYEE_NUMBER == username);
            string staffCode = user.STAFF_CODE;

            var icps = from i in _clinContext.ICPCancer
                       where i.ActOnRefBy != null && i.FinalReviewed == null && (i.GC_CODE == staffCode || i.ToBeReviewedby == username.ToUpper())
                      orderby i.REFERRAL_DATE
                       select i;
            
            return icps.ToList();
        }

        
        public List<ClinicalFacilityList> GetClinicalFacilitiesList() //Get list of all clinic facilities where we hold clinics
        {
            var facs = from f in _clinContext.ClinicalFacilities
                      where f.NON_ACTIVE == 0
                      select f;
        
            return facs.ToList();
        }

        public ICPGeneral GetGeneralICPDetails(int? icpID) //Get details of a general ICP by the IcpID
        {
            var icp = _clinContext.ICPGeneral.FirstOrDefault(c => c.ICPID == icpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetails(int? icpID) //Get details of a cancer ICP by the Cancer ID
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICP_Cancer_ID == icpID);
            return icp;
        }

        public ICPCancer GetCancerICPDetailsByICPID(int? icpID) //Get details of a cancer ICP by the IcpID
        {
            var icp = _clinContext.ICPCancer.FirstOrDefault(c => c.ICPID == icpID);
            return icp;
        }

        public int GetGeneralICPCountByICPID(int id)
        {
            var item = from i in _clinContext.ICPGeneral
                       where i.ICPID == id
                       select i;

            return item.Count();
        }

        public int GetCancerICPCountByICPID(int id)
        {
            var item = from i in _clinContext.ICPCancer
                       where i.ICPID == id
                       select i;

            return item.Count();
        }

    }
}
