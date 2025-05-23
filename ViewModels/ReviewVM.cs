using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace ClinicX.ViewModels
{
    [Keyless]
    public class ReviewVM
    {
        public Patient patient { get; set; }
        public ActivityItem referral { get; set; }
        public List<Appointment> appointmentList { get; set; }
        public List<Referral> referralList { get; set; }
        public List<StaffMember> staffMembers { get; set; }
        public Review review { get; set; }
        public List<Review> reviewList { get; set; }
        public int daysToReview { get; set; }
        public int daysOverdue { get; set; }
    }
}
