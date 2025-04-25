using ClinicX.Data;
using ClinicX.Models;


namespace ClinicX.Meta
{
    interface IBloodFormData
    {        
        public BloodForm GetBloodFormDetails(int id);
        public List<BloodForm> GetBloodFormList(int id);
    }
    public class BloodFormData : IBloodFormData
    {
        private readonly ClinicXContext _cxContext;

        public BloodFormData(ClinicXContext cXContext)
        {
            _cxContext = cXContext;
        }


        public BloodForm GetBloodFormDetails(int id)
        {
            BloodForm frm = _cxContext.BloodForm.FirstOrDefault(t => t.BloodFormID == id);

            return frm;
        }

        public List<BloodForm> GetBloodFormList(int id)
        {
            IQueryable<BloodForm> bforms = _cxContext.BloodForm.Where(f => f.TestID == id);

            return bforms.ToList();
        }

    }
}
