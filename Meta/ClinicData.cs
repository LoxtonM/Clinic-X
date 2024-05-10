using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IClinicData
    {
        public List<Clinics> GetClinicList(string username);
        public List<Clinics> GetClinicByPatientsList(int mpi);
        public Clinics GetClinicDetails(int refID);
        public List<OutcomeList> GetOutcomesList();
        public List<Ethnicity> GetEthnicitiesList();
    }
    public class ClinicData : IClinicData
    {
        private readonly ClinicalContext _clinContext;        
        private readonly StaffUserData _staffUser;

        public ClinicData(ClinicalContext context)
        {
            _clinContext = context;
            _staffUser = new StaffUserData(_clinContext);
        }
        
             

        public List<Clinics> GetClinicList(string username) //Get list of your clinics
        {
            string staffCode = _staffUser.GetStaffMemberDetails(username).STAFF_CODE;

            var clinics = from c in _clinContext.Clinics
                          where c.AppType.Contains("App") && c.STAFF_CODE_1 == staffCode && c.Attendance != "Declined" && !c.Attendance.Contains("Canc")
                          select c;

            return clinics.ToList();
        }

        public List<Clinics> GetClinicByPatientsList(int mpi)
        {
            var appts = from c in _clinContext.Clinics
                        where c.MPI.Equals(mpi)
                        orderby c.BOOKED_DATE descending
                        select c;

            return appts.ToList();
        }

        public Clinics GetClinicDetails(int refID) //Get details of an appointment for display only
        {
            var appt = _clinContext.Clinics.FirstOrDefault(a => a.RefID == refID);

            return appt;
        }

        public List<OutcomeList> GetOutcomesList() //Get list of outcomes for clinic appointments
        {
            var outcomes = from o in _clinContext.Outcomes
                          where o.DEFAULT_CLINIC_STATUS.Equals("Active")
                          select o;

            return outcomes.ToList();
        }

        public List<Ethnicity> GetEthnicitiesList() //Get list of ethnicities
        {
            var ethnicities = from e in _clinContext.Ethnicity
                         orderby e.Ethnic
                         select e;

            return ethnicities.ToList();
        }     
        
    }
}
