using ClinicX.Data;
using ClinicX.Models;
using ClinicX.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicX.Meta
{
    public class PathwayData 
    {
        private readonly ClinicalContext _clinContext;

        public PathwayData(ClinicalContext context)
        {
            _clinContext = context;
        }

        public PatientPathway GetPathwayDetails(int id)
        {
            var pathway = _clinContext.PatientPathway.OrderBy(i => i.REFERRAL_DATE).FirstOrDefault(i => i.MPI == id);
            return pathway;
        } //Get earliest active pathway for patient by MPI

        public List<Pathway> GetPathwayList()
        {
            var pathway = from p in _clinContext.Pathways                         
                         select p;

            return pathway.ToList();
        }

        
    }
}
