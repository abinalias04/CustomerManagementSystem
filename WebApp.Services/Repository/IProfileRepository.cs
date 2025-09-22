using WebApp.Entity.Dto;

namespace WebApp.Services.Repository
{
    public interface IProfileRepository
    {
        Task<bool> CompleteProfileAsync(int userId, CompleteProfileDto dto);
        Task<ProfileDto?> GetProfileAsync(int userId);
        Task<PincodeDetailsDto?> GetPincodeDetailsAsync(string pincodeValue);
    }
}
