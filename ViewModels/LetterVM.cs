using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class LetterVM
    {
        public Patients patient { get; set; }
        public DocumentsContent documentsContent { get; set; }
        public StaffMemberList staffMember { get; set; }
        public ExternalClinician referrer { get; set; }
        public ExternalFacility facility { get; set; }
        public DictatedLetters dictatedLetter { get; set; }
    }
}
