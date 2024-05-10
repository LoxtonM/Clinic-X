using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IAlertData    
    {        
        public List<Alert> GetAlertsList(int id);
    }
    public class AlertData : IAlertData
    {
        private readonly ClinicalContext _clinContext;

        public AlertData(ClinicalContext context)
        {
            _clinContext = context;
        }

        public List<Alert> GetAlertsList(int id) //Get list of alerts for patient by MPI
        {
            var alerts = from a in _clinContext.Alert
                        where a.MPI == id & a.EffectiveToDate == null
                        orderby a.AlertID
                        select a;            

            return alerts.ToList();
        }        
    }
}
