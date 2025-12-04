using Microsoft.Extensions.Logging;
using RVParking.Data;

namespace RVParking.Services.SMS
{
    public class SmsMockService : ISmsService
    {
        private readonly ILogger<SmsMockService> _logger;

        public SmsMockService(ILogger<SmsMockService> logger)
        {
            _logger = logger;
        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            _logger.LogInformation("Mock SMS sending to {To} (From={From}). Message length={Length}.",
                request.To, request.From ?? "<default>", request.Message?.Length ?? 0);

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
