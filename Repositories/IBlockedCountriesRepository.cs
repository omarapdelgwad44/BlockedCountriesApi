namespace BlockedCountriesApi.Repositories
{
    using BlockedCountriesApi.Models;
    using System.Collections.Generic;

    public interface IBlockedCountriesRepository
    {
        bool AddBlockedCountry(Country country);
        bool RemoveBlockedCountry(string countryCode);
        IEnumerable<Country> GetBlockedCountries(int page, int pageSize, string filter);
        int GetTotalBlockedCountries(string filter);
        bool IsCountryBlocked(string countryCode);
        bool AddTemporalBlock(string countryCode, int durationMinutes);
        bool IsTemporallyBlocked(string countryCode);
        void RemoveExpiredTemporalBlocks();
    }
}