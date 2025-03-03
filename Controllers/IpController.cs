namespace BlockedCountriesApi.Controllers
{
    using BlockedCountriesApi.Helpers;
    using BlockedCountriesApi.Models;
    using BlockedCountriesApi.Repositories;
    using BlockedCountriesApi.Services;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/ip")]
    public class IpController : ControllerBase
    {
        private readonly IGeoLocationService _geoLocationService;
        private readonly IBlockedCountriesRepository _blockedCountriesRepository;
        private readonly ILogRepository _logRepository;
        private readonly ILogger<IpController> _logger;

        public IpController(
            IGeoLocationService geoLocationService,
            IBlockedCountriesRepository blockedCountriesRepository,
            ILogRepository logRepository,
            ILogger<IpController> logger)
        {
            _geoLocationService = geoLocationService;
            _blockedCountriesRepository = blockedCountriesRepository;
            _logRepository = logRepository;
            _logger = logger;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string ipAddress = null)
        {
            try
            {
                // If IP address not provided, get the caller's IP
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    ipAddress = IpHelper.GetClientIpAddress(HttpContext);
                    _logger.LogInformation("Using caller's IP address: {IpAddress}", ipAddress);
                }

                var ipDetails = await _geoLocationService.GetIpDetailsAsync(ipAddress);
                return Ok(ipDetails);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid IP address format");
                return BadRequest(new { message = "Invalid IP address format" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up IP {IpAddress}", ipAddress);
                return StatusCode(500, new { message = "Error processing IP lookup" });
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlock()
        {
            try
            {
                // Get caller's IP address
                var ipAddress = IpHelper.GetClientIpAddress(HttpContext);
                _logger.LogInformation("Checking block status for IP: {IpAddress}", ipAddress);

                // Get country information
                var country = await _geoLocationService.GetCountryByIpAsync(ipAddress);

                if (country == null)
                {
                    return StatusCode(500, new { message = "Error fetching country information" });
                }

                // Check if country is blocked
                bool isBlocked = _blockedCountriesRepository.IsCountryBlocked(country.Code);

                // Log the attempt
                _logRepository.LogBlockedAttempt(new BlockedAttemptLog
                {
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = country.Code,
                    IsBlocked = isBlocked,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                });

                var response = new IpBlockCheckResponse
                {
                    IpAddress = ipAddress,
                    CountryCode = country.Code,
                    CountryName = country.Name,
                    IsBlocked = isBlocked
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP block status");
                return StatusCode(500, new { message = "Error checking block status" });
            }
        }
    }
}