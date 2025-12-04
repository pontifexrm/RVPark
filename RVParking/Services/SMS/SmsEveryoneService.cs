using Microsoft.Extensions.Options;
using RVParking.Data;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RVParking.Services.SMS
{
  
    public class SmsEveryoneSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://smseveryone.com/api/campaign";
        public string SenderId { get; set; } = "2310";
    }

    public class SmsEveryoneService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsEveryoneSettings _settings;
        private readonly ILogger<SmsEveryoneService> _logger;

        public SmsEveryoneService(
            HttpClient httpClient,
            IOptions<SmsEveryoneSettings> settings  ,
            ILogger<SmsEveryoneService> logger)
        {
            _httpClient = httpClient;

            _settings = settings.Value;
            _logger = logger;

            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            // Prefer Authorization header when ApiKey is provided as "Basic <token>" (as in Postman).
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                if (_settings.ApiKey.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = _settings.ApiKey.Substring("Basic ".Length);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                }
                else
                {
                    // Fallback to X-API-Key if not a Basic auth value
                    _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
                }
            }

        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            try
            {
                var originator = request.From ?? _settings.SenderId;
                if(request.To.StartsWith("+"))      
                {
                    request.To = request.To.Substring(1);
                }
                var payload = new
                {
                    Message = request.Message,
                    Originator = originator,
                    Destinations = new[] { request.To },
                    Action = "create"
                };
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null,
                    PropertyNameCaseInsensitive = true
                };
                var jsonContent = JsonSerializer.Serialize(payload, jsonOptions);
                _logger.LogDebug("Sending SMS payload: {Payload}", jsonContent);

                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Build the request URI relative to BaseAddress to avoid accidental double segments.
                var requestUri = new Uri(_httpClient.BaseAddress, "campaign/");
                _logger.LogDebug("Posting SMS to {RequestUri} with Originator {Originator}", requestUri, originator);

                using var response = await _httpClient.PostAsync(requestUri, httpContent);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SMS API returned HTTP {StatusCode}. Response body: {ResponseBody}", (int)response.StatusCode, responseJson);
                    return new SmsResponse
                    {
                        Success = false,
                        Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}. See logs for response body."
                    };
                }
                else
                {
                    return new SmsResponse
                    {
                        Success = true,
                        Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}."
                    };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {To}", request.To);
                return new SmsResponse
                {
                    Success = false,
                    Message = $"Failed to send SMS: {ex.Message}"
                };
            }
        }

        public async Task<SmsStatus?> GetSmsStatusAsync(string messageId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"sms/{messageId}/status");
                if (!response.IsSuccessStatusCode)
                    return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SmsStatus>(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SMS status for {MessageId}", messageId);
                return null;
            }
        }

        public async Task<SmsBalance?> GetBalanceAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("account/balance");
                if (!response.IsSuccessStatusCode)
                    return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SmsBalance>(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get account balance");
                return null;
            }
        }

        public async Task<List<SmsStatus>> GetDeliveryReportsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var endpoint = $"reports/delivery?from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return new List<SmsStatus>();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SmsStatus>>(responseJson) ?? new List<SmsStatus>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get delivery reports from {FromDate} to {ToDate}",
                    fromDate, toDate);
                return new List<SmsStatus>();
            }
        }
    }


}

