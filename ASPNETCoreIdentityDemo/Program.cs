using ASPNETCoreIdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASPNETCoreIdentityDemo.Data;
using ASPNETCoreIdentityDemo.Services;

namespace ASPNETCoreIdentityDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Configure Entity Framework Core
            var connectionString = builder.Configuration.GetConnectionString("SQLServerIdentityConnection") ?? throw new InvalidOperationException("Connection string 'SQLServerIdentityConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            //Configuration Identity Services
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
                options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequiredUniqueChars = 4;
                    // Other settings can be configured here
                })
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            //Configure SMS service settings
            builder.Services.AddTransient<ISMSSender, SMSSender>();
            //Configure email service settings
            builder.Services.AddTransient<ISenderEmail, EmailSender>();

            //Configure external signIn  settings
            builder.Services.AddAuthentication().AddGoogle(options =>
              {
                 options.ClientId = builder.Configuration["Google:AppId"]!;
                  options.ClientSecret = builder.Configuration["Google:AppSecret"]!;
                  // You can set other options as needed.
              });

            // Configure the Application Cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
                options.LoginPath = "/Account/Login"; // Set your login path here
                options.AccessDeniedPath = "/Administration/AccessDenied";
            });
            // Configure the policy based authentication 
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configure the HTTP request pipeline.
            var app = builder.Build();
            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Configuring Authentication Middleware to the Request Pipeline
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}