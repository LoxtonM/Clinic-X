﻿using ClinicX.Data;
using ClinicX.Models;
using System.Data;
using ClinicalXPDataConnections.Models;
using ClinicalXPDataConnections.Data;

namespace ClinicX.Meta
{
    interface IICPActionData
    {        
        public List<ICPAction> GetICPCancerActionsList();
        public List<ICPGeneralAction> GetICPGeneralActionsList();
        public List<ICPGeneralAction2> GetICPGeneralActionsList2();
        public List<ICPCancerReviewAction> GetICPCancerReviewActionsList();
        public ICPCancerReviewAction GetICPCancerAction(int id);
        
    }
    public class ICPActionData : IICPActionData
    {
        private readonly ClinicalContext _clinContext;
        private readonly ClinicXContext _cXContext;
        
        public ICPActionData(ClinicalContext context, ClinicXContext cXContext)
        {
            _clinContext = context;
            _cXContext = cXContext;
        }
        
        public List<ICPAction> GetICPCancerActionsList() //Get list of all triage actions for Cancer ICPs
        {
            IQueryable<ICPAction> actions = from a in _cXContext.ICPCancerActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralAction> GetICPGeneralActionsList() //Get list of all "treatpath" items for General ICPs
        {
            IQueryable<ICPGeneralAction> actions = from a in _cXContext.ICPGeneralActionsList
                         where a.InUse == true
                         orderby a.ID
                         select a;
           
            return actions.ToList();
        }

        public List<ICPGeneralAction2> GetICPGeneralActionsList2() //Get list of all "treatpath2" items for General ICPs
        {
            IQueryable<ICPGeneralAction2> actions = from a in _cXContext.ICPGeneralActionsList2
                         where a.InUse == true
                         orderby a.ID
                         select a;
       
            return actions.ToList();
        }

        public List<ICPCancerReviewAction> GetICPCancerReviewActionsList() //Get list of all "treatpath2" items for General ICPs
        {
            IQueryable<ICPCancerReviewAction> actions = from a in _clinContext.ICPCancerReviewActionsList
                          where a.InUse == true
                          orderby a.ListOrder
                          select a;

            return actions.ToList();
        }

        public ICPCancerReviewAction GetICPCancerAction(int id)
        {
            ICPCancerReviewAction action = _clinContext.ICPCancerReviewActionsList.FirstOrDefault(a => a.ID == id);

            return action;
        }
       

    }
}
