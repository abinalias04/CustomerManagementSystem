using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Entity.Dto
{
    public class SendOtpDto
    {
        public string Email { get; set; }
    }

    public class VerifyRegisterDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
