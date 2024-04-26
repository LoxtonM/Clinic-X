using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ClinicalNoteVM
    {
        public ActivityItems activityItem { get; set; }
        public List<ActivityItems> activityItems { get; set; }    
        public Clinics Clinic { get; set; }
        public List<Clinics> Clinics { get; set; }
        public Referrals linkedReferral { get; set; }       
        public List<NoteTypeList> noteTypeList { get; set; }
        public ClinicalNotes clinicalNote { get; set; }
        public List<ClinicalNotes> clinicalNotesList { get; set; }       
        public Patients patient { get; set; }
        public int noteCount { get; set; }
    }
}
