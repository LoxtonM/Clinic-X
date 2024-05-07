using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    public class ReviewData
    {
        private readonly ClinicalContext _clinContext;
        private readonly StaffUserData _staffUser;

        public ReviewData(ClinicalContext context)
        {
            _clinContext = context;
            _staffUser = new StaffUserData(_clinContext);
        }
       

        public List<Reviews> GetReviewsList(string username) 
        {
            string staffCode = _staffUser.GetStaffMemberDetails(username).STAFF_CODE;

            var reviews = from r in _clinContext.Reviews
                          where r.Review_Recipient == staffCode && r.Review_Status == "Pending"
                          orderby r.Planned_Date
                          select r;

            return reviews.ToList();
        }

        public Reviews GetReviewDetails(int id)
        {
            var review = _clinContext.Reviews.FirstOrDefault(r => r.ReviewID == id);

            return review;
        }


        
    }
}
