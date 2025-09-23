using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Entity.Models
{
    public enum UserRole { Customer =1 , Admin =2 }
    public enum UserStatus { Active = 0, Inactive = 1}
    public enum UserBadge { Bronze =0, Silver = 1, Gold =2 , Platinum =3 }

    public enum ReturnStatus { Pending =0, Approved =1, Received =2, Rejected=3 }
    public enum ReturnReason { Defective = 0, Other = 1 }
    public enum OtpStatus {  Pending =0, Verified=1, Expired=2 }
}

    
