using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IStaffUserData
    {
        public StaffMember GetStaffMemberDetails(string suser);
        public List<StaffMember> GetClinicalStaffList();
        public List<StaffMember> GetStaffMemberList();
        public List<string> GetConsultantsList();
        public List<string> GetGCList();
        public List<string> GetSecTeamsList();
    }
    public class StaffUserData : IStaffUserData
    {
        private readonly ClinicalContext _clinContext;       

        public StaffUserData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        public StaffMember GetStaffMemberDetails(string suser) //Get details of a staff member by login name
        {
            StaffMember item = _clinContext.StaffMembers.FirstOrDefault(i => i.EMPLOYEE_NUMBER == suser);
            return item;
        }

        public List<StaffMember> GetClinicalStaffList() //Get list of all clinical staff members currently in post
        {
            IQueryable<StaffMember> clinicians = from s in _clinContext.StaffMembers
                             where s.InPost == true && (s.CLINIC_SCHEDULER_GROUPS == "GC" || s.CLINIC_SCHEDULER_GROUPS == "Consultant" || s.CLINIC_SCHEDULER_GROUPS == "SpR")
                             orderby s.NAME
                             select s;

            return clinicians.ToList();
        }

        public List<StaffMember> GetStaffMemberList() //Get list of all staff members currently in post 
        {
            IQueryable<StaffMember> sm = from s in _clinContext.StaffMembers
                     where s.InPost.Equals(true)
                     orderby s.NAME
                     select s;

            return sm.ToList();
        }

        public List<string> GetConsultantsList() //Get list of all consultants
        {
            IQueryable<string> clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "Consultant"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetGCList() //Get list of all GCs
        {
            IQueryable<string> clinicians = from rf in _clinContext.StaffMembers
                             where rf.InPost == true && rf.CLINIC_SCHEDULER_GROUPS == "GC"
                             orderby rf.NAME
                             select rf.NAME;

            return clinicians.ToList();
        }

        public List<string> GetSecTeamsList() //Get list of all secretarial teams
        {
            IQueryable<string> secteams = from rf in _clinContext.StaffMembers
                           where rf.BILL_ID != null && rf.BILL_ID.Contains("Team")
                           select rf.BILL_ID;

            return secteams.Distinct().ToList();
        }

    }
}
