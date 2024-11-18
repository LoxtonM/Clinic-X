using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ProfileVM
    {
        public StaffMember staffMember { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
