using ManagementAppMVC.Controllers;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace ManagementAppMVC.Services
{
    public class PowerBiService : IPowerBiService
    {
        private readonly ILogger<PowerBiService> _logger;
        private readonly IConfiguration _config;

        public PowerBiService(ILogger<PowerBiService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }


        public async Task<(string EmbedToken, string EmbedUrl, string reportId)> GetEmbedInfoAsync(string accessToken)
        {
            var groupIdStr = _config["PowerBI:WorkspaceId"];
            var reportIdStr = _config["PowerBI:ReportId"];

            if (string.IsNullOrWhiteSpace(groupIdStr))
                throw new ArgumentNullException(nameof(groupIdStr), "PowerBI:WorkspaceId configuration value is missing or empty.");

            if (string.IsNullOrWhiteSpace(reportIdStr))
                throw new ArgumentNullException(nameof(reportIdStr), "PowerBI:ReportId configuration value is missing or empty.");

            Guid groupId = new(groupIdStr);
            Guid reportId = new(reportIdStr);

            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            var client = new PowerBIClient(accessToken);

            // list workspaces just for information
            var workspaces = await client.Groups.GetGroupsAsync();
            foreach (var r in workspaces.Value.Value)
            {
               _logger.LogInformation($"Workspace: {r.Name}, Id: {r.Id}");
            }


            // list reports in workspace just for information
            var reports = await client.Reports.GetReportsInGroupAsync(groupId);
            foreach (var r in reports.Value.Value)
            {
               _logger.LogInformation($"Report: {r.Name}, Id: {r.Id}");
            }

            // Get report
            var report = await client.Reports.GetReportInGroupAsync(groupId, reportId);
            var datasetId = report.Value.DatasetId;

            // Generate embed token
            var request = new GenerateTokenRequestV2();
            request.Reports.Add(new GenerateTokenRequestV2Report(reportId));
            request.Datasets.Add(new GenerateTokenRequestV2Dataset(datasetId));

            var tokenResponse = await client.EmbedToken.GenerateTokenAsync(request);

            return (tokenResponse.Value.Token, report.Value.EmbedUrl, reportIdStr);
        }
    }
}
