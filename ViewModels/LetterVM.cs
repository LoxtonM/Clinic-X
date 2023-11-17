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
        public Referrer referrer { get; set; }
        public Facility facility { get; set; }

    }
}
