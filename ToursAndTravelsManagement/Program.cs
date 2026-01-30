using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Middlewares;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Services.PdfService;
using QuestPDF.Infrastructure;
using ToursAndTravelsManagement.Services.ExcelService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ToursAndTravelsManagement;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();

        // Configure authorization
        //builder.Services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        //    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
        //});

        // Configure Authentication
        //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        //    .AddCookie(options =>
        //    {
        //        options.LoginPath = "/Account/Login";
        //        options.AccessDeniedPath = "/Account/AccessDenied";
        //        options.LogoutPath = "/Account/Logout";
        //        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        //        options.SlidingExpiration = true;
        //    });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy =>
                policy.RequireRole("User"));
            options.AddPolicy("Both", policy =>
                policy.RequireRole("Admin", "User"));
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.LogoutPath = "/Account/Logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog(); // Add Serilog

        builder.Services.AddScoped<DataSeeder>();

        // Add this line to configure the license type
        QuestPDF.Settings.License = LicenseType.Community;
        builder.Services.AddScoped<IPdfService, PdfService>();

        builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Use the global exception handling middleware
        //app.UseMiddleware<ExceptionHandlingMiddleware>();

        //// Use the global error handling middleware
        //app.UseMiddleware<GlobalErrorHandlingMiddleware>();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
