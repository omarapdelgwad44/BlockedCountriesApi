namespace BlockedCountriesApi.Models
{
    public class TemporalBlock
    {
        public string CountryCode { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
