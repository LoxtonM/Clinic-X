using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IExternalClinicianData
    {
        public string GetCCDetails(ExternalClinician referrer);
        public ExternalClinician GetClinicianDetails(string sref);
        public List<ExternalClinician> GetClinicianList();
    }
    public class ExternalClinicianData : IExternalClinicianData
    {
        private readonly ClinicalContext _clinContext;
        
        public ExternalClinicianData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        public string GetCCDetails(ExternalClinician referrer) //Get details of CC address
        {
            string cc = "";
            if (referrer.FACILITY != null) //believe it or not, there are actually some nulls!!!
            {
                var facility = _clinContext.ExternalFacility.FirstOrDefault(f => f.MasterFacilityCode == referrer.FACILITY);

                cc = cc + Environment.NewLine + facility.NAME + Environment.NewLine + facility.ADDRESS + Environment.NewLine
                    + facility.CITY + Environment.NewLine + facility.STATE + Environment.NewLine + facility.ZIP;
            }
            return cc;
        }

        public ExternalClinician GetClinicianDetails(string sref) //Get details of external/referring clinician
        {
            var item = _clinContext.ExternalClinician.FirstOrDefault(f => f.MasterClinicianCode == sref);
            return item;
        }

        public List<ExternalClinician> GetClinicianList() //Get list of all external/referring clinicians
        {
            var clinicians = from rf in _clinContext.ExternalClinician
                             where rf.NON_ACTIVE == 0
                             orderby rf.NAME
                             select rf;

            return clinicians.ToList();
        }        
    }
}
