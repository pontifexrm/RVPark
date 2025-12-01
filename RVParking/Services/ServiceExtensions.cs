using RVParking.Services.Email;

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
