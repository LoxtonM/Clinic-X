using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IReferralData
    {
        public Referrals GetReferralDetails(int id);
        public List<Referrals> GetReferralsList(int id);
    }
    public class ReferralData : IReferralData
    {
        private readonly ClinicalContext _clinContext;

        public ReferralData(ClinicalContext context)
        {
            _clinContext = context;
        }
                
        public Referrals GetReferralDetails(int id) //Get details of referral by RefID
        {
            var referral = _clinContext.Referrals?.FirstOrDefault(i => i.refid == id);
            return referral;
        } 
                
        public List<Referrals> GetReferralsList(int id) //Get list of active referrals for patient by MPI
        {
            var referrals = from r in _clinContext.Referrals
                           where r.MPI == id & r.RefType.Contains("Referral") & r.COMPLETE != "Complete"
                           orderby r.RefDate
                           select r;            

            return referrals.ToList();
        }       
    }
}
