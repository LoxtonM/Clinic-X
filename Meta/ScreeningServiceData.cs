using ClinicX.Data;
using ClinicX.Models;
using System.Data;

namespace ClinicX.Meta
{
    interface IScreeningServiceData
    {
        public ScreeningService GetScreeningServiceDetails(string gpCode);
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
    }
}
