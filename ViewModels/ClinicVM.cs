using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ClinicVM
    {
        public StaffMember staffMember { get; set; }
        public List<StaffMember> staffMembers { get; set; }
        public List<ActivityItem> activityItems { get; set; }
        public ActivityItem activityItem { get; set; }
        public Appointment Clinic { get; set; }
        public Referral linkedReferral { get; set; }
        public List<Outcome> outcomes { get; set; }
        public List<ClinicalNoteType> noteTypeList { get; set; }
        public Note clinicalNotes { get; set; }
        public List<Note> clinicalNotesList { get; set; }
        public List<Ethnicity> ethnicities { get; set; }
        public Patient patient { get; set; }        
        public List<Appointment> pastClinicsList { get; set; }
        public List<Appointment> currentClinicsList { get; set; }
        public List<Appointment> futureClinicsList { get; set; }
        public DateTime pastClinicFilterDate { get; set; }
        public DateTime futureClinicFilterDate { get; set; }
        public bool isClinicOutstanding { get; set; }
        public string seenByString { get; set; }
        public List<Referral> referralsList { get; set; }
        public List<ClinicVenue> venueList { get; set; }
        public List<ActivityType> appTypeList { get; set; }
    }
}
