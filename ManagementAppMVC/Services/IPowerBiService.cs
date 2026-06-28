namespace ManagementAppMVC.Services
{
    public interface IPowerBiService
    {        
        Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfo(string accessToken, string username, string managed_role, string custom_data);
    }

}
