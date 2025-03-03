namespace BlockedCountriesApi.Services
{
    using BlockedCountriesApi.Models;
    using System.Threading.Tasks;

    public interface IGeoLocationService
    {
        Task<Country> GetCountryByIpAsync(string ipAddress);
        Task<IpLookupResponse> GetIpDetailsAsync(string ipAddress);
    }
}