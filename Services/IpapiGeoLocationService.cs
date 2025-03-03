namespace BlockedCountriesApi.Services
{
    using BlockedCountriesApi.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class IpapiGeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<IpapiGeoLocationService> _logger;

        public IpapiGeoLocationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<IpapiGeoLocationService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["IpapiSettings:ApiKey"];
            _logger = logger;
        }

        public async Task<Country> GetCountryByIpAsync(string ipAddress)
        {
            try
            {
                var response = await GetIpDetailsAsync(ipAddress);
                return new Country
                {
                    Code = response.CountryCode,
                    Name = response.CountryName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching country for IP {IpAddress}", ipAddress);
                return null;
            }
        }

        public async Task<IpLookupResponse> GetIpDetailsAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !IsValidIpAddress(ipAddress))
            {
                throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
            }

            try
            {
                var url = $"https://api.ipapi.com/api/{ipAddress}?access_key={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(content);

                return new IpLookupResponse
                {
                    IpAddress = ipAddress,
                    CountryCode = data.country_code,
                    CountryName = data.country_name,
                    ISP = data.connection?.isp ?? "Unknown"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while looking up IP {IpAddress}", ipAddress);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up IP {IpAddress}", ipAddress);
                throw;
            }
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            // IPv4 validation
            var ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ipAddress, ipv4Pattern);
        }
    }
}