using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ClinicVM
    {
        public List<StaffMemberList> staffMembers { get; set; }
        public List<ActivityItems> activityItems { get; set; }
        public ActivityItems activityItem { get; set; }
        public List<OutcomeList> outcomes { get; set; }
        public List<NoteTypeList> noteTypeList { get; set; }
        public ClinicalNotes clinicalNotes { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public Patients patients { get; set; }        
    }
}
