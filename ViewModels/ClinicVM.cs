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
        public Clinics Clinic { get; set; }
        public Referrals linkedReferral { get; set; }
        public List<OutcomeList> outcomes { get; set; }
        public List<NoteTypeList> noteTypeList { get; set; }
        public ClinicalNotes clinicalNotes { get; set; }
        public List<ClinicalNotes> clinicalNotesList { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public Patients patients { get; set; }        
        public List<Clinics> pastClinicsList { get; set; }
        public List<Clinics> currentClinicsList { get; set; }
        public List<Clinics> futureClinicsList { get; set; }
        public DateTime clinicFilterDate { get; set; }
        public bool isClinicOutstanding { get; set; }
    }
}
