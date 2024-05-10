using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IRelativeData
    {
        public List<Relatives> GetRelativesList(int id);
        public Relatives GetRelativeDetails(int relID);
        public List<Relation> GetRelationsList();
        public List<Gender> GetGenderList();
    }
    public class RelativeData : IRelativeData
    {
        private readonly ClinicalContext _clinContext;      

        public RelativeData(ClinicalContext context)
        {
            _clinContext = context;
        }
                

        public List<Relatives> GetRelativesList(int id) //Get list of relatives of patient by MPI
        {
            var patient = _clinContext.Patients.FirstOrDefault(i => i.MPI == id);
            int wmfacsID = patient.WMFACSID;

            var relative = from r in _clinContext.Relatives
                           where r.WMFACSID == wmfacsID
                           select r;           

            return relative.ToList();
        }

        public Relatives GetRelativeDetails(int relID)
        {
            var rel = _clinContext.Relatives.FirstOrDefault(r => r.relsid == relID);

            return rel;
        }

        public List<Relation> GetRelationsList()
        {
            var item = from i in _clinContext.Relations
                       select i;

            return item.ToList();
        }

        public List<Gender> GetGenderList()
        {
            var item = from i in _clinContext.Genders
                       select i;

            return item.ToList();
        }
    }
}
