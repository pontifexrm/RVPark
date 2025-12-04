using Microsoft.Extensions.Configuration;
using RVParking.Services.Email;
using RVParking.Services.SMS;

namespace RVParking.Services
{
    public static class ServiceExtensions
    {
        // Method to register SMTP implementation
        public static IServiceCollection AddSmtpEmailService(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
            return services;
        }
        public static IServiceCollection AddTNZEmailService(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, TNZEmailService>();
            return services;
        }
        // Method to register MailKit implementation
        public static IServiceCollection AddMailKitEmailService(this IServiceCollection services)
        {
//            var settings = configuration.GetSection("MailKitSettings").Get<MailKitSettings>();
            services.AddScoped<IEmailService, MailKitEmailService>();
            return services;
        }
        public static IServiceCollection AddSmsTNZService(this IServiceCollection services)
        {
            services.AddScoped<ISmsService, SmsTNZService>();
            return services;
        }
        public static IServiceCollection AddSmsEveryoneService(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind options from configuration section "SmsEveryone"
            services.Configure<SmsEveryoneSettings>(configuration.GetSection("SmsEveryone"));

            // Register the typed HTTP client which will resolve IOptions<SmsEveryoneSettings> in SmsEveryoneService
            services.AddHttpClient<ISmsService, SmsEveryoneService>();
            return services;
        }
        public static IServiceCollection AddSmsMockService(this IServiceCollection services)
        {
            services.AddScoped<ISmsService, SmsMockService>();
            return services;
        }

        // Method to register SendGrid implementation
        //public static IServiceCollection AddSendGridEmailService(this IServiceCollection services, IConfiguration configuration)
        //{
        //    // Cost is US$20/month as of Nov 2025 for 100,000 emails/month
        //    var settings = configuration.GetSection("SendGridSettings").Get<SendGridSettings>();

        //    services.AddSingleton<ISendGridClient>(_ => new SendGridClient(settings?.ApiKey));
        //    services.AddScoped<IEmailService, SendGridEmailService>();

        //    return services;
        //}

        // Method to register Mock implementation
        public static IServiceCollection AddMockEmailService(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, MockEmailService>();
            return services;
        }

    }
}
