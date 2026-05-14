namespace ManagementAppMVC.Services
{
    public interface IAuthService
    {
        public Task<string> GetAccessTokenAsync();
    }
}
