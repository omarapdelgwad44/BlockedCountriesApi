namespace BlockedCountriesApi.Controllers
{
    using BlockedCountriesApi.Models;
    using BlockedCountriesApi.Repositories;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly ILogRepository _logRepository;
        private readonly ILogger<LogsController> _logger;

        public LogsController(
            ILogRepository logRepository,
            ILogger<LogsController> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize < 1)
                    pageSize = 10;

                if (pageSize > 100)
                    pageSize = 100;

                var logs = _logRepository.GetBlockedAttempts(page, pageSize);
                var totalCount = _logRepository.GetTotalBlockedAttempts();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new PagedResponse<BlockedAttemptLog>
                {
                    Items = logs,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blocked attempts logs");
                return StatusCode(500, new { message = "Error retrieving logs" });
            }
        }
    }
}