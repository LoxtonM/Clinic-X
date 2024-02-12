using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class DictatedLetterVM
    {
        public List<DictatedLettersCopies> dictatedLettersCopies  { get; set; }
        public List<DictatedLettersPatients> dictatedLettersPatients { get; set; }
        public DictatedLetters dictatedLetters { get; set; }
        public List<Patients> patients { get; set; }
        public List<StaffMemberList> staffMemberList { get; set;}
        public Patients patientDetails { get; set; }
        //public Referrals referralDetails { get; set; }
        public ActivityItems activityDetails { get; set; }
        public ExternalFacility referrerFacility { get; set; }
        public ExternalClinician referrer { get; set; }
        public ExternalFacility GPFacility { get; set; }
        public List<ExternalFacility> facilities { get; set; }
        public List<ExternalClinician> clinicians { get; set; }
        public List<string> consultants { get; set; }
        public List<string> gcs { get; set; }
        public List<string> secteams { get; set; }
    }
}
