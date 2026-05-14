using Microsoft.Identity.Client;

namespace ManagementAppMVC.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> GetAccessTokenAsync()
        {            
            var clientId = _config["AzureAd:ClientId"];
            var tenantId = _config["AzureAd:TenantId"];
            var clientSecret = _config["AzureAd:ClientSecret"];

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var scopes = new[] { "https://analysis.windows.net/powerbi/api/.default" };

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(authority)
                .Build();

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }
    }
}
