using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public interface IPurchaseRepository
    {
        Task<PurchaseResultDto?> CompletePurchaseAsync(int userId);
        Task<PagedResult<PurchaseResultDto>> GetAllPurchasesAsync(PurchaseQueryParameters parameters, int? purchaseId = null);
        Task<PurchaseResultDto?> GetPurchaseByIdAsync(int purchaseId);
        Task<PagedResult<PurchaseResultDto>> GetMyPurchasesAsync(int userId, PurchaseQueryParameters parameters);
    }
}
