namespace BlockedCountriesApi.Controllers
{
    using BlockedCountriesApi.Models;
    using BlockedCountriesApi.Repositories;
    using BlockedCountriesApi.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly IBlockedCountriesRepository _repository;
        private readonly IGeoLocationService _geoLocationService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            IBlockedCountriesRepository repository,
            IGeoLocationService geoLocationService,
            ILogger<CountriesController> logger)
        {
            _repository = repository;
            _geoLocationService = geoLocationService;
            _logger = logger;
        }

        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] CountryBlockRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CountryCode))
            {
                return BadRequest("Country code is required");
            }

            var country = new Country
            {
                Code = request.CountryCode.ToUpperInvariant(),
                Name = null // Name will be fetched later if needed
            };

            if (_repository.AddBlockedCountry(country))
            {
                _logger.LogInformation("Country {CountryCode} added to blocked list", country.Code);
                return Ok(new { message = $"Country {country.Code} blocked successfully" });
            }

            return Conflict(new { message = $"Country {country.Code} is already blocked" });
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return BadRequest("Country code is required");
            }

            if (_repository.RemoveBlockedCountry(countryCode))
            {
                _logger.LogInformation("Country {CountryCode} removed from blocked list", countryCode);
                return Ok(new { message = $"Country {countryCode} unblocked successfully" });
            }

            return NotFound(new { message = $"Country {countryCode} is not in the blocked list" });
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string filter = "")
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var countries = _repository.GetBlockedCountries(page, pageSize, filter);
            var totalCount = _repository.GetTotalBlockedCountries(filter);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var response = new PagedResponse<Country>
            {
                Items = countries,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Ok(response);
        }

        [HttpPost("temporal-block")]
        public IActionResult TemporalBlockCountry([FromBody] TemporalBlockRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.CountryCode))
            {
                return BadRequest("Country code is required");
            }

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
            {
                return BadRequest("Duration must be between 1 and 1440 minutes");
            }

            // Check if country is already permanently blocked
            if (_repository.IsCountryBlocked(request.CountryCode))
            {
                return Conflict(new { message = $"Country {request.CountryCode} is already blocked" });
            }

            // Check if country is already temporarily blocked
            if (_repository.IsTemporallyBlocked(request.CountryCode))
            {
                return Conflict(new { message = $"Country {request.CountryCode} is already temporarily blocked" });
            }

            if (_repository.AddTemporalBlock(request.CountryCode, request.DurationMinutes))
            {
                _logger.LogInformation("Country {CountryCode} temporarily blocked for {Duration} minutes",
                    request.CountryCode, request.DurationMinutes);

                return Ok(new
                {
                    message = $"Country {request.CountryCode} temporarily blocked for {request.DurationMinutes} minutes",
                    expiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes)
                });
            }

            return BadRequest(new { message = "Failed to block country temporarily" });
        }
    }
}