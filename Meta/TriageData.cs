using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class TriageData
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

        public List<ICPActionsList> GetICPCancerActionsList() //Get list of all triage actions for Cancer ICPs
        {
            var actions = from a in _clinContext.ICPCancerActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList> GetICPGeneralActionsList() //Get list of all "treatpath" items for General ICPs
        {
            var actions = from a in _clinContext.ICPGeneralActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralActionsList2> GetICPGeneralActionsList2() //Get list of all "treatpath2" items for General ICPs
        {
            var actions = from a in _clinContext.ICPGeneralActionsList2
                         where a.InUse == true
                         orderby a.ID
                         select a;
       
            return actions.ToList();
        }

        public List<ICPCancerReviewActionsList> GetICPCancerReviewActionsList() //Get list of all "treatpath2" items for General ICPs
        {
            var actions = from a in _clinContext.ICPCancerReviewActionsList
                          where a.InUse == true
                          orderby a.ListOrder
                          select a;

            return actions.ToList();
        }

        public ICPCancerReviewActionsList GetICPCancerAction(int id)
        {
            var action = _clinContext.ICPCancerReviewActionsList.FirstOrDefault(a => a.ID == id);

            return action;
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
