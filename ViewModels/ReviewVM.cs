﻿using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

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
