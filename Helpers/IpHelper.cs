namespace BlockedCountriesApi.Helpers
{
    using Microsoft.AspNetCore.Http;
    using System.Net;

    public static class IpHelper
    {
        public static string GetClientIpAddress(HttpContext context)
        {
            string ip = null;

            // Try to get IP from X-Forwarded-For header
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                ip = forwardedHeader.Split(',')[0].Trim();
            }

            // If not available, get from remote address
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }

            // If we're still null (unlikely), return a default
            if (string.IsNullOrEmpty(ip))
            {
                ip = "127.0.0.1";
            }

            return ip;
        }
    }
}
