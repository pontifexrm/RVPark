using RVPark.Data;


namespace RVPark.Services.SMS
{
  
        public interface ISmsService
        {
            Task<SmsResponse> SendSmsAsync(SmsRequest request);
            Task<SmsStatus?> GetSmsStatusAsync(string messageId);
            Task<SmsBalance?> GetBalanceAsync();
            Task<List<SmsStatus>> GetDeliveryReportsAsync(DateTime fromDate, DateTime toDate);
        }
 
}
