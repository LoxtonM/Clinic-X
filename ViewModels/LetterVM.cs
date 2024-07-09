using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class LetterVM
    {
        public Patient patient { get; set; }
        public DocumentsContent documentsContent { get; set; }
        public StaffMember staffMember { get; set; }
        public ExternalClinician referrer { get; set; }
        public ExternalFacility facility { get; set; }
        public DictatedLetter dictatedLetter { get; set; }
    }
}
