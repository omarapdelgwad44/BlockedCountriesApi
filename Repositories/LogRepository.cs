namespace BlockedCountriesApi.Repositories
{
    using BlockedCountriesApi.Models;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class LogRepository : ILogRepository
    {
        private readonly ConcurrentBag<BlockedAttemptLog> _logs = new();

        public void LogBlockedAttempt(BlockedAttemptLog log)
        {
            if (log != null)
            {
                _logs.Add(log);
            }
        }

        public IEnumerable<BlockedAttemptLog> GetBlockedAttempts(int page, int pageSize)
        {
            return _logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalBlockedAttempts()
        {
            return _logs.Count;
        }
    }
}