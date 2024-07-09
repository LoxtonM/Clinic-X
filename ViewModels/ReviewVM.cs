using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicX.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ReviewVM
    {
        public Patient patient { get; set; }
        public ActivityItem referrals { get; set; }
        public List<StaffMember> staffMembers { get; set; }
        public Review review { get; set; }
        public List<Review> reviewList { get; set; }
    }
}
