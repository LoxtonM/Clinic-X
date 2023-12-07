using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ReviewVM
    {
        public Patients patient { get; set; }
        public ActivityItems referrals { get; set; }
        public List<StaffMemberList> staffMembers { get; set; }
    }
}
