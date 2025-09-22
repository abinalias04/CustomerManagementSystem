using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
        public interface IAuthRepository
        {
            Task<string> SendOtpAsync(SendOtpDto dto);
            Task<string> VerifyAndRegisterAsync(VerifyRegisterDto dto);
            Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        }
}
