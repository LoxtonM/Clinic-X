using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IStaffOptionsDataAsync
    {
        Task<StaffOptions> GetStaffOptions(string staffCode);
    }
    public class StaffOptionsDataAsync : IStaffOptionsDataAsync
    {
        private readonly ClinicXContext _context;

        public StaffOptionsDataAsync(ClinicXContext context)
        {
            _context = context;
        }

        public async Task<StaffOptions> GetStaffOptions(string staffCode)
        {
            StaffOptions options = await _context.StaffOptions.FirstAsync(o => o.Staff_Code == staffCode);

            return options;
        }
    }
}
