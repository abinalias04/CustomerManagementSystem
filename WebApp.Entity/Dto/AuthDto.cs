using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Entity.Models;

namespace WebApp.Entity.Dto
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public int  UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public List<MenuDto> Menus { get; set; } = new();
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }

}
