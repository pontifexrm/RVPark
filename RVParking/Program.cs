using AutoMapper;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVParking.Components;
using RVParking.Components.Account;
using RVParking.Components.Admin;
using RVParking.Data;
using RVParking.Services;
using RVParking.Services.Email;
using RVParking.Services.Environment;
using RVParking.Services.Logging;
using Syncfusion.Blazor;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<Bkg_UserService>();

// Request localization options (en-NZ only)
var supportedCultures = new[] { new CultureInfo("en-NZ") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-NZ");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
// Antiforgery configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
}); 


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options => {
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(3));

// Load configuration files based on the environment
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql((connectionString), new MySqlServerVersion(new Version(8, 0, 40))), ServiceLifetime.Scoped);
// Make DbContextOptions a singleton so the singleton IDbContextFactory can consume it,
// while keeping the actual DbContext as scoped for request/Identity usage.
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    options.UseSqlServer(connectionString),
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);

//builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

// Primary registration - DbContextFactory
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppLogger, AppLogger>();
// Select email provider from configuration
var emailProvider = builder.Configuration["EmailSettings:Provider"]?.Trim() ?? "Mock";
switch (emailProvider.ToLowerInvariant())
{
    case "smtp":
        builder.Services.AddSmtpEmailService();
        break;

    case "mailkit":
        builder.Services.AddMailKitEmailService();
        break;

    case "tnz":
        builder.Services.AddTNZEmailService();
        break;

    default:
        builder.Services.AddMockEmailService();
        break;
}
// Select emSMSail provider from configuration
var smsProvider = builder.Configuration["SmsSettings:Provider"]?.Trim() ?? "Mock";
switch (smsProvider.ToLowerInvariant())
{
    case "smseveryone":
        builder.Services.AddSmsEveryoneService(builder.Configuration);
        break;

    case "smstnz":
        builder.Services.AddSmsTNZService();
        break;

    default:
        builder.Services.AddSmsMockService();
        break;
}
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();


//builder.Services.AddSingleton<IEnvironmentInfoService, EnvironmentInfoService>();
builder.Services.AddScoped<IEnvironmentInfoService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
    return new EnvironmentInfoService(env, configuration, dbContext);
});
//builder.Services.AddScoped<IAppLogger, AppLogger>();


// Secondary registration for scaffolding
//builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
//{
//    var factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
//    using var context = factory.CreateDbContext();
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//}, ServiceLifetime.Scoped);


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("AuthMessageSenderOptions"));
//builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();

// Register IHttpContextAccessor and the custom service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessorService>();

// Add AutoMapper
//builder.Services.AddAutoMapper(typeof(MappingProfile));

// Blazor Syncfusion
builder.Services.AddSyncfusionBlazor(options => { });//         Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1ccnRRQmReV0x+W0pWYEs=
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5cd3RWRmRdV0NwX0dWYEg= ");
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1ccnRRQmReV0x+W0pWYEs=");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Migrate database if needed Only do it in debug mode as in Production we will manually manage this process.
//#if DEBUG
//using (var scope = app.Services.CreateScope())
//{
//        //var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//        //dbContext.Database.Migrate();
//    }
//#endif

// Use request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();


app.Run();
