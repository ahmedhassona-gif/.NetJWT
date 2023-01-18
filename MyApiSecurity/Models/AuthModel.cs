namespace MyApiSecurity.Models
{
    public class AuthModel
    {
        public string Message { get; set; }
        public bool isAuthentcated { get; set; }
        public string UserName { get; set; }
        public string Email { set; get; }
        public List<string> Roles { get; set; }
        public string Token { set; get; }
        public DateTime ExpiresOn { set; get; }
    }
}
