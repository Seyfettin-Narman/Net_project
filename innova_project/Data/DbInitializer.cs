using innova_project.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace innova_project.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(MasrafTakipContext context)
        {
            context.Database.EnsureCreated();

            
            if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            {
                return; 
            }

            var admin = new User
            {
                Name = "Admin",
                Email = "admin@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("AdminPassword123"),
                Role = "Admin"
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}