using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class HPOVM
    {
        public List<ClinicalNotes> clinicalNotes { get; set; }
        public ClinicalNotes clinicalNote { get; set; }
        public List<HPOTermDetails> hpoTermDetails { get; set; }
        public List<HPOTerms> hpoTerms { get; set; }
        public List<HPOExtractVM> hpoExtractVM { get; set;}          
    }
}
