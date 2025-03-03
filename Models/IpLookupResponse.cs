namespace BlockedCountriesApi.Models
{
    public class IpLookupResponse
    {
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string ISP { get; set; }
    }
}
