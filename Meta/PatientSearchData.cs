using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface IPatientSearchData
    {        
        public List<Patients> GetPatientsListByCGUNo(string? cguNo);
        public List<Patients> GetPatientsListByName(string? firstname, string? lastname);
        public List<Patients> GetPatientsListByNHS(string? nhsNo);
        public List<Patients> GetPatientsListByDOB(DateTime dob);
        //the reason for multiple "GetPatientsLists", and not one with multiple parameters, is because in order to do that,
        //the "patients" list would have to be created first and then narrowed by criteria.
        //This would result in very long loading times, as there are a LOT of patients, and I don't really want to select them all
        //only to have to filter them.
    }
    public class PatientSearchData : IPatientSearchData
    {
        private readonly ClinicalContext _clinContext;       

        public PatientSearchData(ClinicalContext context)
        {
            _clinContext = context;
        }       
               
        
        public List<Patients> GetPatientsListByCGUNo(string cguNo)
        {
            var patients = _clinContext.Patients.Where(p => p.CGU_No.Contains(cguNo));            
            
            return patients.ToList();
        }
        public List<Patients> GetPatientsListByName(string? firstname, string? lastname)
        {
            var patients = _clinContext.Patients.Where(p => p.FIRSTNAME.Contains(firstname) || p.LASTNAME.Contains(lastname));
            
            return patients.ToList();
        }

        public List<Patients> GetPatientsListByNHS(string nhsNo)
        {
            var patients = _clinContext.Patients.Where(p => p.SOCIAL_SECURITY.Contains(nhsNo));

            return patients.ToList();
        }

        public List<Patients> GetPatientsListByDOB(DateTime dob)        
        {
            var patients = _clinContext.Patients.Where(p => p.DOB == dob);

            return patients.ToList();
        }
    }
}
