namespace BlockedCountriesApi.Repositories
{
    using BlockedCountriesApi.Models;
    using System.Collections.Generic;

    public interface ILogRepository
    {
        void LogBlockedAttempt(BlockedAttemptLog log);
        IEnumerable<BlockedAttemptLog> GetBlockedAttempts(int page, int pageSize);
        int GetTotalBlockedAttempts();
    }
}