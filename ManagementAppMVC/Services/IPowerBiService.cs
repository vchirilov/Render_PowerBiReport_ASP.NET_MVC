namespace ManagementAppMVC.Services
{
    public interface IPowerBiService
    {
        Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoAsync( string accessToken);
    }

}
