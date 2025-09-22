using System.Threading.Tasks;
using WebApp.Entity.Dto;

namespace WebApp.Api.Repositories
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId);
    }
}
