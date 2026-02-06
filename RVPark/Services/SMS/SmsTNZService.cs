using RVPark.Data;
using RVPark.Services.Environment;
using RVPark.Services.Logging;
using TNZAPI.NET.Core;

namespace RVPark.Services.SMS
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
        private readonly IAppLogger _appLogger;
        private readonly IEnvironmentInfoService _env;

        public SmsTNZService(IConfiguration configuration, IAppLogger applogger, IEnvironmentInfoService env)
        {
            _settings = configuration.GetSection("TNZ").Get<SmsTNZSettings>()
           ?? throw new InvalidOperationException("TNZ settings not configured");
            _appLogger = applogger;
            _env = env;

        }
        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            // Using TNZ
            var apiUser = new TNZApiUser()  // this shoild come from configuration injection
            {
                AuthToken = _settings.AuthToken
            };
            var client = new TNZApiClient(apiUser);
            request.Message = _env.ShouldDisplayEnvInfo ? $"TEST-{request.Message}" : request.Message;
            var response = client.Messaging.SMS.SendMessage
                (
                    destination: request.To,
                    messageText: request.Message
                );
            // prepare applog message
            var alogMsg = $"SmsTo: {request.To} fm: {_settings.SMSrept} Msg: {request.Message}";
            alogMsg = _env.ShouldDisplayEnvInfo ? $"TEST-{alogMsg}" : alogMsg;
            await _appLogger.LogAsync("Info", $"SMSEveryone", alogMsg);

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
