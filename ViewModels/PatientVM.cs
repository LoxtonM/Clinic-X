using ClinicX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ClinicX.Models.Relatives;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class PatientVM
    {
        public Patients patient { get; set; }
        public List<Relatives> relatives { get; set; }
        public List<HPOTermDetails> hpoTermDetails { get; set; }        
        public List<Referrals> referrals { get; set; }
        public PatientPathway patientPathway { get; set; }
        public List<Alert> alerts { get; set; }
    }
}
