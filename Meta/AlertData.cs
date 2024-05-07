using ClinicX.Data;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicX.Meta
{
    public class AlertData 
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
