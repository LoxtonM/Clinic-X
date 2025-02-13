using ClinicX.Data;
using ClinicX.Models;

namespace ClinicX.Meta
{
    interface IScreeningServiceData
    {
        public ScreeningService GetScreeningServiceDetails(string gpCode);
        public ScreeningService GetScreeningServiceDetailsByCode(string ssCode);
        public List<ScreeningService> GetScreeningServiceList();
    }
    public class ScreeningServiceData : IScreeningServiceData
    {
        private readonly ClinicXContext _clinContext;
        
        public ScreeningServiceData(ClinicXContext context)
        {
            _clinContext = context;
        }
                
        public ScreeningService GetScreeningServiceDetails(string gpCode)
        {
            ScreeningServiceGPCode serviceCode = _clinContext.ScreeningServiceGPCode.FirstOrDefault(r => r.GPCode == gpCode);

            ScreeningService service = _clinContext.ScreeningService.FirstOrDefault(s => s.ScreeningOfficeCode == serviceCode.ScreeningOfficeCode);

            return service;
        }

        public ScreeningService GetScreeningServiceDetailsByCode(string ssCode)
        {
            ScreeningService service = _clinContext.ScreeningService.FirstOrDefault(s => s.ScreeningOfficeCode == ssCode);

            return service;
        }

        public List<ScreeningService> GetScreeningServiceList()
        {
            IQueryable<ScreeningService> screeningServices = from s in _clinContext.ScreeningService
                                                             select s;

            return screeningServices.ToList();
        }
    }
}
