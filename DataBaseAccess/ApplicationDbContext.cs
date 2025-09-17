using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModelClasses;
using ModelClasses.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAccess
{
    public class ApplicationDbContext :IdentityDbContext
    {
  
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {
            }

            public DbSet<Category> categories { get; set; }
            public DbSet<ApplicationUser> applicationUser { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<PImage> PImages { get; set; }
            public DbSet<Inventory> Inventories { get; set; }

            public DbSet<UserCart> UserCarts { get; set; }

            public DbSet<UserOrder> Orders { get; set; }
            public DbSet<OrderDetails> Details { get; set; }
    }
}
