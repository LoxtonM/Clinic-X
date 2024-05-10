using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IPathwayData
    {
        public PatientPathway GetPathwayDetails(int id);
        public List<Pathway> GetPathwayList();
    }
    public class PathwayData : IPathwayData
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
