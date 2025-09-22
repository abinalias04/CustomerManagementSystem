using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Entity.Models
{
    public class Menu
    {
        public int MenuId { get; set; }

        public string Name { get; set; } = string.Empty; 
        public string Path { get; set; } = string.Empty; 

        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    }


    public class RoleMenu
    {
        public int RoleMenuId { get; set; }

        // FK to ASP.NET Identity Role
        public int RoleId { get; set; }
        public IdentityRole<int> Role { get; set; }

        // FK to Menu
        public int MenuId { get; set; }
        public Menu Menu { get; set; }
    }
}
