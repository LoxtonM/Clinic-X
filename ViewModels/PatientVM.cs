using ClinicalXPDataConnections.Models;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class PatientVM
    {
        public Patient patient { get; set; }
        public Patient nextPatient { get; set; }
        public Patient previousPatient { get; set; }
        public List<Relative> relatives { get; set; }
        public List<HPOTermDetails> hpoTermDetails { get; set; }        
        public List<Referral> referrals { get; set; }
        public List<Referral> referralsActiveGeneral { get; set; }
        public List<Referral> referralsActiveCancer { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
        public List<Diary> diary { get; set; }
        public StaffMember staffMember { get; set; }
        public bool ptSuccess { get; set; }
        public string message { get; set; }
        public bool isPhenotipsAvailable { get; set; }
        public int currentAge { get; set; }
        public bool isCancerPPQScheduled { get; set; }
        public bool isGeneralPPQScheduled { get; set; }
        public bool isPatientInPhenotips { get; set; }
        public int cancerICPID { get; set; }
        public List<ICPCancer> icpCancerList { get; set; }
        public string edmsLink { get; set; }
    }
}
