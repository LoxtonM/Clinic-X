using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IICPActionData
    {        
        public List<ICPActionsList> GetICPCancerActionsList();
        public List<ICPGeneralActionsList> GetICPGeneralActionsList();
        public List<ICPGeneralActionsList2> GetICPGeneralActionsList2();
        public List<ICPCancerReviewActionsList> GetICPCancerReviewActionsList();
        public ICPCancerReviewActionsList GetICPCancerAction(int id);
        
    }
    public class ICPActionData : IICPActionData
    {
        private readonly ClinicalContext _clinContext;
        
        public ICPActionData(ClinicalContext context)
        {
            _clinContext = context;           
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
       

    }
}
