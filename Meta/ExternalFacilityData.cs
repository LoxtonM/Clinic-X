using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class ExternalFacilityData
    {
        private readonly ClinicalContext _clinContext;
      
        public ExternalFacilityData(ClinicalContext context)
        {
            _clinContext = context;
        }
        

        public ExternalFacility GetFacilityDetails(string sref) //Get details of external/referring facility
        {
            var item = _clinContext.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == sref);
            return item;
        }        

        public List<ExternalFacility> GetFacilityList() //Get list of all external/referring facilities
        {
            var facilities = from rf in _clinContext.ExternalFacility
                             where rf.NONACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return facilities.ToList();
        }

    }
}
