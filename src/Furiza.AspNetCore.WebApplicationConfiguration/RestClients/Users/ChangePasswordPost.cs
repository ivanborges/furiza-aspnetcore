namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients.Users
{
    public class ChangePasswordPost
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}