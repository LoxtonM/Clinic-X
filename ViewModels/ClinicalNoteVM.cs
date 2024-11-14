using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ClinicalNoteVM
    {
        public ActivityItem activityItem { get; set; }
        public List<ActivityItem> activityItems { get; set; }    
        public Appointment Clinic { get; set; }
        public List<Appointment> Clinics { get; set; }
        public Referral linkedReferral { get; set; }       
        public List<ClinicalNoteType> noteTypeList { get; set; }
        public Note clinicalNote { get; set; }
        public List<Note> clinicalNotesList { get; set; }       
        public Patient patient { get; set; }
        public int noteCount { get; set; }
    }
}
