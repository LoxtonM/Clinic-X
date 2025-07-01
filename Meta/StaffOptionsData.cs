using ClinicX.Data;
using ClinicX.Models;

namespace ClinicX.Meta
{
    interface IStaffOptionsData
    {
        StaffOptions GetStaffOptions(string staffCode);
    }
    public class StaffOptionsData : IStaffOptionsData
    {
        private readonly ClinicXContext _context;

        public StaffOptionsData(ClinicXContext context)
        {
            _context = context;
        }

        public StaffOptions GetStaffOptions(string staffCode)
        {
            StaffOptions options = _context.StaffOptions.First(o => o.Staff_Code == staffCode);

            return options;
        }
    }
}
