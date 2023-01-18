namespace MyApiSecurity.Helper
{
    public class JWT
    {
        public string key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double DurationInMinute { get; set; }

    }
}
