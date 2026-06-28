namespace ManagementAppMVC.Services
{
    public interface IPowerBiService
    {
        Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoAsync( string accessToken);
        Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoWithFiltersAsync(string accessToken, string username, Dictionary<string, string> rlsFilters);
    }

}
