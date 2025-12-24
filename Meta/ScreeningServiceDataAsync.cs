using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IScreeningServiceDataAsync
    {
        public Task<ScreeningService> GetScreeningServiceDetails(string gpCode);
        public Task<ScreeningService> GetScreeningServiceDetailsByCode(string ssCode);
        public Task<List<ScreeningService>> GetScreeningServiceList();
    }
    public class ScreeningServiceDataAsync : IScreeningServiceDataAsync
    {
        private readonly ClinicXContext _clinContext;
        
        public ScreeningServiceDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }
                
        public async Task<ScreeningService> GetScreeningServiceDetails(string gpCode)
        {
            ScreeningServiceGPCode serviceCode = await _clinContext.ScreeningServiceGPCode.FirstOrDefaultAsync(r => r.GPCode == gpCode);

            if (serviceCode == null) { return null; } //for if the GP isn't on the list

            ScreeningService service = await _clinContext.ScreeningService.FirstAsync(s => s.ScreeningOfficeCode == serviceCode.ScreeningOfficeCode);

            return service;
        }

        public async Task<ScreeningService> GetScreeningServiceDetailsByCode(string ssCode)
        {
            ScreeningService service = await _clinContext.ScreeningService.FirstAsync(s => s.ScreeningOfficeCode == ssCode);

            return service;
        }

        public async Task<List<ScreeningService>> GetScreeningServiceList()
        {
            IQueryable<ScreeningService> screeningServices = from s in _clinContext.ScreeningService
                                                             select s;

            return await screeningServices.ToListAsync();
        }
    }
}
