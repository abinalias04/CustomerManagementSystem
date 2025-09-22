using WebApp.Entity.Dto;

namespace WebApp.Services.Repository
{
    public interface IReturnRepository
    {
        Task<int> CreateReturnRequestAsync(CreateReturnRequestDto dto, int userId);
        Task<bool> ApproveReturnRequestAsync(int returnRequestId, int adminId);
        Task<ReturnSummaryDto?> CompleteReturnAsync(CompleteReturnDto dto, int adminId);
        Task<PagedResult<ReturnResultDto>> GetAllReturnsAsync(ReturnQueryParameters parameters);
        Task<ReturnResultDto?> GetReturnByIdAsync(int id);
        Task<PagedResult<ReturnResultDto>> GetMyReturnsAsync(int userId, ReturnQueryParameters parameters);
    }
}

