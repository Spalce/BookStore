using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Category> Categories { get; set; }

        private async Task SeedData(int noOfData)
        {
            var names = new List<string>() { "Philemon", "Spalce", "Bernice", "Ewura", "Abednego" };
            List<int> rate = new List<int>() { 1, 2, 3, 4, 5 };
            var random = new Random();
            var items = new List<Book>();

            // for (int i = 1; i <= noOfData; i++)
            // {
            //     var item = new Book()
            //     {
            //         Id = i,
            //         Subject = $"Subject {i}",
            //         Message = $"Message {i}",
            //         Rating = rate[random.Next(0, rate.Count)],
            //         CreatedBy = names[random.Next(0, names.Count)],
            //         CreatedDate = DateTime.Today.AddDays(i)
            //     };
            //     items.Add(item);
            // }

            await Books.AddRangeAsync(items);
            await SaveChangesAsync();
        }
    }
}
