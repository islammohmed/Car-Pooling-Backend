
using Car_Pooling.Data;
using Car_Pooling.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Car_Pooling
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext> (options => options.UseSqlServer (builder.Configuration.GetConnectionString ("DefaultConnection")));
            // make email unigue to ignore  username
            builder.Services.AddIdentity<User, IdentityRole> (options =>
            {
                options.User.RequireUniqueEmail = true; 
            }).AddEntityFrameworkStores<AppDbContext> ().AddDefaultTokenProviders ();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
