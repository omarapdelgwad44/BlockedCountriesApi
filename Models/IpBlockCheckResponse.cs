namespace BlockedCountriesApi.Models
{
    public class IpBlockCheckResponse
    {
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public bool IsBlocked { get; set; }
    }
}
