using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface IPatientData
    {
        public Patient GetPatientDetails(int id);
        public Patient GetPatientDetailsByWMFACSID(int id);        
    }
    public class PatientData : IPatientData 
    {
        private readonly ClinicalContext _clinContext;       

        public PatientData(ClinicalContext context)
        {
            _clinContext = context;
        }       
        
        public Patient GetPatientDetails(int id)
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.MPI == id);
            return patient;
        } //Get patient details from MPI

        public Patient GetPatientDetailsByWMFACSID(int id)
        {
            Patient patient = _clinContext.Patients.FirstOrDefault(i => i.WMFACSID == id);
            return patient;
        } //Get patient details from WMFACSID               
        
    }
}
