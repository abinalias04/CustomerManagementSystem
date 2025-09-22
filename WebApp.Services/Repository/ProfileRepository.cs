using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            return await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new ProfileDto
                {
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    StateName = u.State.StateName,
                    DistrictName = u.District.DistrictName,
                    Pincode = u.Pincode.PincodeValue,
                    PostOfficeName = u.PostOffice.PostOfficeName,
                    Gender = u.Gender,
                    DateOfBirth = u.DateOfBirth,
                    PostOfficeId = u.PostOfficeId ?? 0
                    

                })
                .FirstOrDefaultAsync();
        }

        public async Task<PincodeDetailsDto?> GetPincodeDetailsAsync(string pincodeValue)
        {
            var pincode = await _context.Pincodes
                .Include(p => p.District)
                    .ThenInclude(d => d.State)
                .Include(p => p.PostOffices)
                .FirstOrDefaultAsync(p => p.PincodeValue == pincodeValue);

            if (pincode == null) return null;

            return new PincodeDetailsDto
            {
                PincodeId = pincode.PincodeId,
                PincodeValue = pincode.PincodeValue,
                StateId = pincode.District.State.StateId,
                StateName = pincode.District.State.StateName,
                DistrictId = pincode.District.DistrictId,
                DistrictName = pincode.District.DistrictName,
                PostOffices = pincode.PostOffices
                    .Select(po => new PostOfficeDto
                    {
                        Id = po.PostOfficeId,
                        Name = po.PostOfficeName
                    })
                    .ToList()
            };
        }

        public async Task<bool> CompleteProfileAsync(int userId, CompleteProfileDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            // Lookup Pincode + related data
            var pincode = await _context.Pincodes
                .Include(p => p.District)
                    .ThenInclude(d => d.State)
                .Include(p => p.PostOffices)
                .FirstOrDefaultAsync(p => p.PincodeValue == dto.Pincode);

            if (pincode == null) throw new Exception("Invalid Pincode");

            // Pick post office (user may select or fallback to default first one)
            var postOffice = pincode.PostOffices.FirstOrDefault(po => po.PostOfficeId == dto.PostOfficeId)
                             ?? pincode.PostOffices.FirstOrDefault();

            if (postOffice == null) throw new Exception("No PostOffice available for this Pincode");

            // Update User
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;

            user.StateId = pincode.District.State.StateId;
            user.DistrictId = pincode.District.DistrictId;
            user.PincodeId = pincode.PincodeId;
            user.PostOfficeId = postOffice.PostOfficeId;

            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;
            user.IsProfileCompleted = true;

            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}
