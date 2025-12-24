using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;


namespace ClinicX.Meta
{
    public interface IBloodFormDataAsync
    {        
        public Task<BloodForm> GetBloodFormDetails(int id);
        public Task<List<BloodForm>> GetBloodFormList(int id);
    }
    public class BloodFormDataAsync : IBloodFormDataAsync
    {
        private readonly ClinicXContext _cxContext;

        public BloodFormDataAsync(ClinicXContext cXContext)
        {
            _cxContext = cXContext;
        }


        public async Task<BloodForm> GetBloodFormDetails(int id)
        {
            BloodForm frm = await _cxContext.BloodForm.FirstAsync(t => t.BloodFormID == id);

            return frm;
        }

        public async Task<List<BloodForm>> GetBloodFormList(int id)
        {
            IQueryable<BloodForm> bforms = _cxContext.BloodForm.Where(f => f.TestID == id);

            return await bforms.ToListAsync();
        }

    }
}
