using MainChapar.Data;
using MainChapar.Helpers;
using MainChapar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MainChapar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson();

            builder.Services.AddAuthentication();

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>()
                .AddErrorDescriber<IdentityErrorDescriber>();

            builder.Services.AddAuthorization(option =>
            {
                option.AddPolicy("Test", policy =>
                {
                    policy.RequireClaim("testest", "itis");
                });
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/User/AccessDenied";
                options.LoginPath = "/User/Login";
            });
            builder.Services.AddSession();
           

            var app = builder.Build();

          
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "user", "admin" };

                foreach (var roleName in roleNames)
                {
                    var roleExists = roleManager.RoleExistsAsync(roleName).Result;
                    if (!roleExists)
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
               name: "areas",
               pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "productsByCategory",
                pattern: "products/{slug}",
                defaults: new { controller = "Products", action = "ByCategory" });

            app.Run();
        }
    }
}
