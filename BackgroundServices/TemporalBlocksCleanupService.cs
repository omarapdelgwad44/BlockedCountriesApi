namespace BlockedCountriesApi.BackgroundServices
{
    using BlockedCountriesApi.Repositories;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TemporalBlocksCleanupService : BackgroundService
    {
        private readonly IBlockedCountriesRepository _repository;
        private readonly ILogger<TemporalBlocksCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public TemporalBlocksCleanupService(
            IBlockedCountriesRepository repository,
            ILogger<TemporalBlocksCleanupService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temporal blocks cleanup service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking for expired temporal blocks");
                    _repository.RemoveExpiredTemporalBlocks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while removing expired temporal blocks");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Temporal blocks cleanup service is stopping");
        }
    }
}