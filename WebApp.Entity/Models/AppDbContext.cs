using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;
using WebApp.Entity.Models;

namespace WebApp.Entity.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ----------------- DbSets -----------------
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Pincode> Pincodes { get; set; }
        public DbSet<PostOffice> PostOffices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ----------------- Enum as string -----------------
            builder.Entity<ApplicationUser>()
                 .Property(u => u.Role)
                 .HasConversion<int>();   

            builder.Entity<ApplicationUser>()
                .Property(u => u.Status)
                .HasConversion<int>();

            builder.Entity<ApplicationUser>()
                .Property(u => u.Badge)
                .HasConversion<int>();

            builder.Entity<ReturnRequest>()
                .Property(r => r.Reason)
                .HasConversion<int>();

            builder.Entity<ReturnRequest>()
                .Property(r => r.Status)
                .HasConversion<int>();


            // ----------------- Relationships -----------------

            builder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .HasMany(p => p.PurchaseItems)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasMany(p => p.PurchaseItems)
                .WithOne(pi => pi.Purchase)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseItem>()
                .HasMany(pi => pi.ReturnItems)
                .WithOne(ri => ri.PurchaseItem)
                .HasForeignKey(ri => ri.PurchaseItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReturnRequest>()
                .HasMany(rr => rr.ReturnItems)
                .WithOne(ri => ri.ReturnRequest)
                .HasForeignKey(ri => ri.ReturnRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReturnRequest>()
                .HasOne(rr => rr.ApprovedByUser)
                .WithMany()
                .HasForeignKey(rr => rr.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReturnRequest>()
                .HasOne(rr => rr.ReceivedByUser)
                .WithMany()
                .HasForeignKey(rr => rr.ReceivedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<State>()
                .HasIndex(s => s.StateName)
                .IsUnique();

            builder.Entity<District>()
                .HasIndex(d => new { d.StateId, d.DistrictName })
                .IsUnique();

            builder.Entity<Pincode>()
                .HasIndex(p => new { p.DistrictId, p.PincodeValue })
                .IsUnique();

            builder.Entity<District>()
                .HasOne(d => d.State)
                .WithMany(s => s.Districts)
                .HasForeignKey(d => d.StateId);

            builder.Entity<Pincode>()
                .HasOne(p => p.District)
                .WithMany(d => d.Pincodes)
                .HasForeignKey(p => p.DistrictId);

            builder.Entity<PostOffice>()
                .HasOne(po => po.Pincode)
                .WithMany(p => p.PostOffices)
                .HasForeignKey(po => po.PincodeId);

            // RoleMenu → Role
            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany()
                .HasForeignKey(rm => rm.RoleId);

            // RoleMenu → Menu
            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);


            //seed Roles 
            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>
                {
                    Id = (int)UserRole.Customer,   // 0
                    Name = UserRole.Customer.ToString(),
                    NormalizedName = UserRole.Customer.ToString().ToUpper()
                },
                new IdentityRole<int>
                {
                    Id = (int)UserRole.Admin,      // 1
                    Name = UserRole.Admin.ToString(),
                    NormalizedName = UserRole.Admin.ToString().ToUpper()
                }
            );



            // Seed Menus
            builder.Entity<Menu>().HasData(
                // Customer 

                new Menu { MenuId = 2, Name = "Customer Dashboard", Path = "/customerdashboard" },
                 new Menu { MenuId = 1, Name = "Product List", Path = "/customer/products" },
                new Menu { MenuId = 3, Name = "Cart", Path = "/customer/viewproduct" },
                new Menu { MenuId = 4, Name = "Purchase History", Path = "/customer/purchasehistory" },
                new Menu { MenuId = 5, Name = "My Returns", Path = "/customer/returnrequest" },
                new Menu { MenuId = 6, Name = "Edit Info", Path = "/customer/profilecompletion" },

                // Admin
                new Menu { MenuId = 7, Name = "Admin Dashboard", Path = "/dashboard" },
                new Menu { MenuId = 8, Name = "Product Management", Path = "/productform" },
                new Menu { MenuId = 9, Name = "Customer Management", Path = "/customermanagement" },
                new Menu { MenuId = 10, Name = "Request Approvals", Path = "/approverequest" }
            );

            // Seed RoleMenu (assuming you already have seeded roles with Ids like 'admin-role-id' and 'customer-role-id')
            builder.Entity<RoleMenu>().HasData(
           // Customer menus (display order: Customer Dashboard first)
           new RoleMenu { RoleMenuId = 1, RoleId = 1, MenuId = 2 }, // Customer Dashboard
           new RoleMenu { RoleMenuId = 2, RoleId = 1, MenuId = 1 }, // Product List
           new RoleMenu { RoleMenuId = 3, RoleId = 1, MenuId = 3 }, // Cart
           new RoleMenu { RoleMenuId = 4, RoleId = 1, MenuId = 4 }, // Purchase History
           new RoleMenu { RoleMenuId = 5, RoleId = 1, MenuId = 5 }, // My Returns
           new RoleMenu { RoleMenuId = 6, RoleId = 1, MenuId = 6 }, // Edit Info

           // Admin menus
           new RoleMenu { RoleMenuId = 7, RoleId = 2, MenuId = 7 },  // Admin Dashboard
           new RoleMenu { RoleMenuId = 8, RoleId = 2, MenuId = 8 },  // Product Management
           new RoleMenu { RoleMenuId = 9, RoleId = 2, MenuId = 9 },  // Customer Management
           new RoleMenu { RoleMenuId = 10, RoleId = 2, MenuId = 10 } // Request Approvals
       );




            // ----------------- Indexes -----------------
            builder.Entity<CartItem>().HasIndex(ci => ci.ProductId);
            builder.Entity<PurchaseItem>().HasIndex(pi => pi.ProductId);
            builder.Entity<ReturnItem>().HasIndex(ri => ri.PurchaseItemId);

            // ------------------ Decimal Precision ------------------
            builder.Entity<ApplicationUser>().Property(u => u.NetSpend).HasPrecision(18, 2);
            builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
           // builder.Entity<Purchase>().Property(p => p.GrossTotal).HasPrecision(18, 2);
            builder.Entity<Purchase>().Property(p => p.NetTotal).HasPrecision(18, 2);
            builder.Entity<PurchaseItem>().Property(pi => pi.UnitPriceAtPurchase).HasPrecision(18, 2);
            builder.Entity<ReturnItem>().Property(ri => ri.RefundLineTotal).HasPrecision(18, 2);
            builder.Entity<ReturnRequest>().Property(rr => rr.RefundAmount).HasPrecision(18, 2);
        }
    }
}
