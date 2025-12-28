using SOLFranceBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace SOLFranceBackend.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartHeader> CartHeaders { get; set; }
        public DbSet<CartDetails> CartDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Product>().HasData(new Product
            //{
            //    ProductId = 1,
            //    Name = "Samosa",
            //    Price = 15,
            //    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
            //    ImageUrl = "https://placehold.co/603x403",
            //    CategoryName = "Appetizer"
            //});
            //modelBuilder.Entity<Product>().HasData(new Product
            //{
            //    ProductId = 2,
            //    Name = "Paneer Tikka",
            //    Price = 13.99,
            //    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
            //    ImageUrl = "https://placehold.co/602x402",
            //    CategoryName = "Appetizer"
            //});
            //modelBuilder.Entity<Product>().HasData(new Product
            //{
            //    ProductId = 3,
            //    Name = "Sweet Pie",
            //    Price = 10.99,
            //    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
            //    ImageUrl = "https://placehold.co/601x401",
            //    CategoryName = "Dessert"
            //});
            //modelBuilder.Entity<Product>().HasData(new Product
            //{
            //    ProductId = 4,
            //    Name = "Pav Bhaji",
            //    Price = 15,
            //    Description = " Quisque vel lacus ac magna, vehicula sagittis ut non lacus.<br/> Vestibulum arcu turpis, maximus malesuada neque. Phasellus commodo cursus pretium.",
            //    ImageUrl = "https://placehold.co/600x400",
            //    CategoryName = "Entree"
            //});


            modelBuilder.Entity<CartDetails>()
                .HasOne(p => p.CartHeader)
                .WithMany(pd => pd.CartDetailsList)
                .HasForeignKey(p => p.CartHeaderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
