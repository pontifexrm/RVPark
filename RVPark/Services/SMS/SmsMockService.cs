using Microsoft.Extensions.Logging;
using RVPark.Data;
using RVPark.Services.Environment;
using RVPark.Services.Logging;

namespace RVPark.Services.SMS
{
    public class SmsMockService : ISmsService
    {
        private readonly ILogger<SmsMockService> _logger;
        private readonly IEnvironmentInfoService _env;
        private readonly IAppLogger _applogger;

        public SmsMockService(ILogger<SmsMockService> logger, IEnvironmentInfoService env, IAppLogger appLogger)
        {
            _logger = logger;
            _env = env;
            _applogger = appLogger;
        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            _logger.LogInformation("Mock SMS sending to {To} (From={From}). Message length={Length}.",
                request.To, request.From ?? "<default>", request.Message?.Length ?? 0);
            // prepare applog message
            var logMessage = $"[Mock SMS] To: {request.To}, From: {request.From ?? "<default>"}, Message: {request.Message}";
            if (_env.ShouldDisplayEnvInfo)
            {
                logMessage = $"TEST-{logMessage}";
            }
            await _applogger.LogAsync("Info", "MockSMSService", logMessage);


            // simulate latency
            await Task.Delay(50);

            var response = new SmsResponse
            {
                MessageId = Guid.NewGuid().ToString(),
                Message = "Mock SMS sent successfully",
                Success = true
            };

            _logger.LogDebug("Mock SMS sent. MessageId={MessageId}", response.MessageId);
            return response;
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
