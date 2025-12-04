using Microsoft.Extensions.Configuration;
using RVParking.Data;
using TNZAPI.NET.Core;

namespace RVParking.Services.SMS
{
    public class SmsTNZSettings
    {
            public string AuthToken { get; set; } = string.Empty;
            public string fromEmail { get; set; } = string.Empty;
            public string CCemail { get; set; } = string.Empty;
            public string SMSrept { get; set; } = string.Empty;

    }



    public class SmsTNZService : ISmsService
    {

        private readonly SmsTNZSettings _settings;
        public SmsTNZService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("TNZ").Get<SmsTNZSettings>()
           ?? throw new InvalidOperationException("TNZ settings not configured");
        }
        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            // Using TNZ
            var apiUser = new TNZApiUser()  // this shoild come from configuration injection
            {
                AuthToken = _settings.AuthToken
            };
            var client = new TNZApiClient(apiUser);

            var response = client.Messaging.SMS.SendMessage
                (
                    destination: request.To,
                    messageText: request.Message
                );
            return await Task.FromResult(new SmsResponse
            {
                MessageId = response.MessageID,
                Message = "Sent",
                Success = true
            });
        }

        Task<SmsBalance?> ISmsService.GetBalanceAsync()
        {
            throw new NotImplementedException();
        }

        Task<List<SmsStatus>> ISmsService.GetDeliveryReportsAsync(DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        Task<SmsStatus?> ISmsService.GetSmsStatusAsync(string messageId)
        {
            throw new NotImplementedException();
        }
    }
}
