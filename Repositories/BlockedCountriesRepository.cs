namespace BlockedCountriesApi.Repositories
{
    using BlockedCountriesApi.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class BlockedCountriesRepository : IBlockedCountriesRepository
    {
        private readonly ConcurrentDictionary<string, Country> _blockedCountries = new();
        private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new();

        public bool AddBlockedCountry(Country country)
        {
            if (country == null || string.IsNullOrWhiteSpace(country.Code))
                return false;

            // Standardize country code
            country.Code = country.Code.ToUpperInvariant();

            // Check if already exists
            if (_blockedCountries.ContainsKey(country.Code))
                return false;

            return _blockedCountries.TryAdd(country.Code, country);
        }

        public bool RemoveBlockedCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return false;

            countryCode = countryCode.ToUpperInvariant();
            return _blockedCountries.TryRemove(countryCode, out _);
        }

        public IEnumerable<Country> GetBlockedCountries(int page, int pageSize, string filter)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            // Apply filter if provided
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToUpperInvariant();
                query = query.Where(c =>
                    c.Code.Contains(filter) ||
                    (c.Name != null && c.Name.ToUpperInvariant().Contains(filter))
                );
            }

            // Apply pagination
            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalBlockedCountries(string filter)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            // Apply filter if provided
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToUpperInvariant();
                query = query.Where(c =>
                    c.Code.Contains(filter) ||
                    (c.Name != null && c.Name.ToUpperInvariant().Contains(filter))
                );
            }

            return query.Count();
        }

        public bool IsCountryBlocked(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return false;

            countryCode = countryCode.ToUpperInvariant();

            // Check permanent blocks
            if (_blockedCountries.ContainsKey(countryCode))
                return true;

            // Check temporal blocks
            return IsTemporallyBlocked(countryCode);
        }

        public bool AddTemporalBlock(string countryCode, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || durationMinutes < 1 || durationMinutes > 1440)
                return false;

            countryCode = countryCode.ToUpperInvariant();

            // Check if already temporarily blocked
            if (_temporalBlocks.ContainsKey(countryCode))
                return false;

            var block = new TemporalBlock
            {
                CountryCode = countryCode,
                ExpirationTime = DateTime.UtcNow.AddMinutes(durationMinutes)
            };

            return _temporalBlocks.TryAdd(countryCode, block);
        }

        public bool IsTemporallyBlocked(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return false;

            countryCode = countryCode.ToUpperInvariant();

            if (_temporalBlocks.TryGetValue(countryCode, out var block))
            {
                // If block has expired, remove it
                if (block.ExpirationTime <= DateTime.UtcNow)
                {
                    _temporalBlocks.TryRemove(countryCode, out _);
                    return false;
                }
                return true;
            }

            return false;
        }

        public void RemoveExpiredTemporalBlocks()
        {
            var now = DateTime.UtcNow;
            var expiredBlocks = _temporalBlocks
                .Where(b => b.Value.ExpirationTime <= now)
                .Select(b => b.Key)
                .ToList();

            foreach (var code in expiredBlocks)
            {
                _temporalBlocks.TryRemove(code, out _);
            }
        }
    }
}