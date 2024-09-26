using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class HPOVM
    {
        public List<Note> clinicalNotes { get; set; }
        public Note clinicalNote { get; set; }
        public List<HPOTermDetails> hpoTermDetails { get; set; }
        public List<HPOTerm> hpoTerms { get; set; }
        public List<HPOExtractVM> hpoExtractVM { get; set;}
        public StaffMember staffMember { get; set; }
        public string searchTerm { get; set; }
    }
}
