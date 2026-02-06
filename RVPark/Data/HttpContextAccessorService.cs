namespace RVPark.Data
{
    public class HttpContextAccessorService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            var publicIpAddress = context?.Request?.Headers["X-Real-IP"] ?? "Unknown";
            if (publicIpAddress.ToString() == "")
            {
                var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
                if (remoteIpAddress == null)
                {
                    return publicIpAddress.ToString();
                }

                // Check if the address is IPv4 or IPv6
                if (remoteIpAddress.IsIPv4MappedToIPv6)
                {
                    // Convert IPv4-mapped IPv6 address to IPv4
                    return remoteIpAddress.MapToIPv4().ToString();
                }
                else
                {
                    return remoteIpAddress.ToString();
                }
            }
            return publicIpAddress.ToString();
        }
    }
}
