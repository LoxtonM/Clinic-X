using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class StaffUserData 
    {
        private readonly ClinicalContext _clinContext;       

        public StaffUserData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        public StaffMemberList GetStaffMemberDetails(string suser) //Get details of a staff member by login name
        {
            var item = _clinContext.StaffMembers.FirstOrDefault(i => i.EMPLOYEE_NUMBER == suser);
            return item;
        }

        public List<StaffMemberList> GetClinicalStaffList() //Get list of all clinical staff members currently in post
        {
            var clinicians = from s in _clinContext.StaffMembers
                             where s.InPost == true && (s.CLINIC_SCHEDULER_GROUPS == "GC" || s.CLINIC_SCHEDULER_GROUPS == "Consultant")
                             orderby s.NAME
                             select s;

            return clinicians.ToList();
        }

        public List<StaffMemberList> GetStaffMemberList() //Get list of all staff members currently in post 
        {
            var sm = from s in _clinContext.StaffMembers
                     where s.InPost.Equals(true)
                     orderby s.NAME
                     select s;

            return sm.ToList();
        }

        public List<string> GetConsultantsList() //Get list of all consultants
        {
            var clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "Consultant"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetGCList() //Get list of all GCs
        {
            var clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "GC"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetSecTeamsList() //Get list of all secretarial teams
        {
            var secteams = from rf in _clinContext.StaffMembers
                           where rf.BILL_ID != null && rf.BILL_ID.Contains("Team")
                           select rf.BILL_ID;

            return secteams.Distinct().ToList();
        }

    }
}
