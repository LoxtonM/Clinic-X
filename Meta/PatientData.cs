using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface IPatientData
    {
        public Patients GetPatientDetails(int id);
        public Patients GetPatientDetailsByWMFACSID(int id);
    }
    public class PatientData : IPatientData //This contains all of the data "get" functions that retrieve data from a data model.
                        //They each return either a list or a data model.
        //there are two constructors because there are two possible "datacontext" parameters that can be passed to it.
    {
        private readonly ClinicalContext _clinContext;       

        public PatientData(ClinicalContext context)
        {
            _clinContext = context;
        }       
        
        public Patients GetPatientDetails(int id)
        {
            var patient = _clinContext.Patients?.FirstOrDefault(i => i.MPI == id);
            return patient;
        } //Get patient details from MPI

        public Patients GetPatientDetailsByWMFACSID(int id)
        {
            var patient = _clinContext.Patients?.FirstOrDefault(i => i.WMFACSID == id);
            return patient;
        } //Get patient details from WMFACSID        
        
    }
}
