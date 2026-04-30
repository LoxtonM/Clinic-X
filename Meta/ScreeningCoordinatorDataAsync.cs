using ClinicX.Data;
using ClinicX.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicX.Meta
{
    public interface IScreeningCoordinatorDataAsync
    {
        public Task<List<ScreeningCoordinator>> GetScreeningCoordinatorList();       
    }
    public class ScreeningCoordinatorDataAsync : IScreeningCoordinatorDataAsync
    {
        private readonly ClinicXContext _clinContext;

        public ScreeningCoordinatorDataAsync(ClinicXContext context)
        {
            _clinContext = context;
        }

        public async Task<List<ScreeningCoordinator>> GetScreeningCoordinatorList()
        {            
            IQueryable<ScreeningCoordinator> screeningCoordinatorList = _clinContext.ScreeningCoordinator.AsNoTracking();

            return await screeningCoordinatorList.ToListAsync();
        }       
    }
}
